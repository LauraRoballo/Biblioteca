using System.Globalization;
using BibliotecaLiceista.Web.Models;
using BibliotecaLiceista.Web.Repositories;
using Microsoft.Extensions.Options;

namespace BibliotecaLiceista.Web.Services
{
    /// <summary>
    /// Reglas de negocio del módulo de préstamos.
    /// Separa la lógica (qué se permite) del acceso a datos (cómo se guarda), que está en PrestamoRepository.
    /// Reglas aplicadas:
    /// RN-01 máximo de libros prestados según el rol,
    /// RN-02 días máximos de préstamo según el rol,
    /// RN-03 solo se prestan ejemplares disponibles de libros activos,
    /// y la multa por cada día de retraso en la devolución.
    /// </summary>
    public class PrestamoService
    {
        private static readonly CultureInfo CulturaColombia = new("es-CO");

        private readonly PrestamoRepository _prestamoRepository;
        private readonly UsuarioRepository _usuarioRepository;
        private readonly PoliticasPrestamo _politicas;

        public PrestamoService(PrestamoRepository prestamoRepository, UsuarioRepository usuarioRepository,
            IOptions<PoliticasPrestamo> opcionesPoliticas)
        {
            _prestamoRepository = prestamoRepository;
            _usuarioRepository = usuarioRepository;
            _politicas = opcionesPoliticas.Value;
        }

        /// <summary>Valor de la multa por día de retraso, para mostrarlo en pantalla.</summary>
        public decimal MultaPorDia => _politicas.MultaPorDia;

        /// <summary>
        /// Días máximos de préstamo según el rol (RN-02). Devuelve 0 para los roles que no reciben préstamos.
        /// </summary>
        public int ObtenerDiasMaximos(string? nombreRol) => nombreRol switch
        {
            RolesUsuario.Alumno => _politicas.DiasAlumno,
            RolesUsuario.Docente => _politicas.DiasDocente,
            _ => 0
        };

        /// <summary>Indica si un rol puede recibir préstamos (solo alumnos y docentes).</summary>
        public static bool PuedeRecibirPrestamos(string? nombreRol) =>
            nombreRol == RolesUsuario.Alumno || nombreRol == RolesUsuario.Docente;

        /// <summary>
        /// Calcula los días de retraso y la multa por ejemplar entre la fecha límite y la de devolución.
        /// </summary>
        public (int DiasRetraso, decimal MultaPorEjemplar) CalcularMulta(DateTime fechaLimite, DateTime fechaDevolucion)
        {
            int diasRetraso = Math.Max(0, (fechaDevolucion.Date - fechaLimite.Date).Days);
            return (diasRetraso, diasRetraso * _politicas.MultaPorDia);
        }

        /// <summary>Da formato de pesos colombianos a un valor, por ejemplo $ 3.000.</summary>
        public static string FormatearPesos(decimal valor) => valor.ToString("C0", CulturaColombia);

        /// <summary>
        /// Valida las reglas de negocio de un préstamo nuevo y, si se cumplen, lo registra.
        /// </summary>
        /// <param name="formulario">Datos diligenciados por el bibliotecario.</param>
        /// <param name="idBibliotecario">Usuario que registra el préstamo (el que inició sesión).</param>
        public async Task<ResultadoOperacion> RegistrarPrestamoAsync(NuevoPrestamoFormulario formulario, int? idBibliotecario)
        {
            var resultado = new ResultadoOperacion();
            DateTime hoy = DateTime.Today;

            // El usuario debe existir y ser alumno o docente
            var usuario = await _usuarioRepository.ConsultarPorIdAsync(formulario.IDUsuario);
            if (usuario is null || !PuedeRecibirPrestamos(usuario.NombreTipoUsuario))
            {
                resultado.Errores.Add("Solo se pueden registrar préstamos a alumnos y docentes.");
                return resultado;
            }

            // Al menos un ejemplar, sin repetidos
            var listaIdsEjemplares = formulario.IDsEjemplares.Distinct().ToList();
            if (listaIdsEjemplares.Count == 0)
            {
                resultado.Errores.Add("Seleccione al menos un ejemplar para prestar.");
            }

            // RN-01: máximo de libros según el rol
            var listaTipos = await _usuarioRepository.ConsultarTiposUsuarioAsync();
            int maximoLibros = listaTipos.FirstOrDefault(t => t.IDTipoUsuario == usuario.IDTipoUsuario)?.MaxLibros ?? 0;
            int librosPrestados = await _prestamoRepository.ContarLibrosPrestadosAsync(usuario.IDUsuario);
            if (librosPrestados + listaIdsEjemplares.Count > maximoLibros)
            {
                int disponiblesParaPrestar = Math.Max(0, maximoLibros - librosPrestados);
                resultado.Errores.Add(
                    $"{usuario.Nombre} {usuario.Apellido} ({usuario.NombreTipoUsuario}) ya tiene {librosPrestados} libro(s) prestado(s) " +
                    $"y su máximo es {maximoLibros}. Puede llevar {disponiblesParaPrestar} más.");
            }

            // RN-02: fecha límite posterior a hoy y dentro de los días permitidos para el rol
            int diasMaximos = ObtenerDiasMaximos(usuario.NombreTipoUsuario);
            DateTime fechaLimiteMaxima = hoy.AddDays(diasMaximos);
            DateTime fechaLimite = formulario.FechaLimite!.Value.Date;

            if (fechaLimite <= hoy)
            {
                resultado.Errores.Add("La fecha límite debe ser posterior a la fecha del préstamo (hoy).");
            }
            else if (fechaLimite > fechaLimiteMaxima)
            {
                resultado.Errores.Add(
                    $"Para un {usuario.NombreTipoUsuario?.ToLower()} el préstamo máximo es de {diasMaximos} días: " +
                    $"la fecha límite no puede pasar del {fechaLimiteMaxima:dd/MM/yyyy}.");
            }

            if (!resultado.Exitoso)
            {
                return resultado;
            }

            var prestamo = new Prestamo
            {
                IDUsuario = usuario.IDUsuario,
                FechaPrestamo = DateTime.Now,
                FechaLimite = fechaLimite,
                IDBibliotecario = idBibliotecario
            };

            try
            {
                // RN-03 se vuelve a comprobar dentro de la transacción del repositorio
                int idPrestamo = await _prestamoRepository.RegistrarAsync(prestamo, listaIdsEjemplares);
                resultado.Mensaje =
                    $"Préstamo #{idPrestamo} registrado: {listaIdsEjemplares.Count} libro(s) para {usuario.Nombre} {usuario.Apellido}, " +
                    $"con devolución hasta el {fechaLimite:dd/MM/yyyy}.";
            }
            catch (InvalidOperationException error)
            {
                resultado.Errores.Add(error.Message);
            }

            return resultado;
        }

        /// <summary>
        /// Valida y registra la devolución de un préstamo, calculando la multa por retraso.
        /// </summary>
        public async Task<ResultadoOperacion> RegistrarDevolucionAsync(PrestamoResumen prestamo, DevolucionFormulario formulario)
        {
            var resultado = new ResultadoOperacion();
            DateTime hoy = DateTime.Today;
            DateTime fechaDevolucion = formulario.FechaDevolucion!.Value.Date;

            // La devolución no puede ser anterior al préstamo ni una fecha futura
            if (fechaDevolucion < prestamo.FechaPrestamo.Date)
            {
                resultado.Errores.Add(
                    $"La fecha de devolución no puede ser anterior a la fecha del préstamo ({prestamo.FechaPrestamo:dd/MM/yyyy}).");
            }
            if (fechaDevolucion > hoy)
            {
                resultado.Errores.Add("La fecha de devolución no puede ser una fecha futura.");
            }
            if (!resultado.Exitoso)
            {
                return resultado;
            }

            var (diasRetraso, multaPorEjemplar) = CalcularMulta(prestamo.FechaLimite, fechaDevolucion);

            try
            {
                int cantidadEjemplares = await _prestamoRepository.RegistrarDevolucionAsync(
                    prestamo.IDPrestamo, fechaDevolucion, formulario.Observaciones, multaPorEjemplar);

                resultado.Mensaje = diasRetraso == 0
                    ? $"Devolución del préstamo #{prestamo.IDPrestamo} registrada a tiempo. Sin multa."
                    : $"Devolución del préstamo #{prestamo.IDPrestamo} registrada con {diasRetraso} día(s) de retraso. " +
                      $"Multa total: {FormatearPesos(multaPorEjemplar * cantidadEjemplares)}.";
            }
            catch (InvalidOperationException error)
            {
                resultado.Errores.Add(error.Message);
            }

            return resultado;
        }

        /// <summary>Anula un préstamo vigente registrado por error.</summary>
        public async Task<ResultadoOperacion> AnularPrestamoAsync(int idPrestamo)
        {
            var resultado = new ResultadoOperacion();
            try
            {
                await _prestamoRepository.AnularAsync(idPrestamo);
                resultado.Mensaje = $"El préstamo #{idPrestamo} fue anulado y sus ejemplares quedaron disponibles.";
            }
            catch (InvalidOperationException error)
            {
                resultado.Errores.Add(error.Message);
            }
            return resultado;
        }
    }
}
