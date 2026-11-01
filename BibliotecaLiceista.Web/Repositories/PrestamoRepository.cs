using System.Text;
using BibliotecaLiceista.Web.Data;
using BibliotecaLiceista.Web.Models;
using Microsoft.Data.SqlClient;

namespace BibliotecaLiceista.Web.Repositories
{
    /// <summary>
    /// Acceso a datos de préstamos y devoluciones con ADO.NET.
    /// Las operaciones que modifican varias tablas usan una transacción (SqlTransaction):
    /// si un paso falla se deshacen todos, y la base de datos nunca queda a medias.
    /// </summary>
    public class PrestamoRepository
    {
        private readonly ConexionBd _conexionBd;

        public PrestamoRepository(ConexionBd conexionBd)
        {
            _conexionBd = conexionBd;
        }

        /// <summary>
        /// Cambia a "Retrasado" los préstamos vigentes cuya fecha límite ya pasó.
        /// Se ejecuta cada vez que se abre la pantalla de préstamos.
        /// </summary>
        /// <returns>Cantidad de préstamos que pasaron a "Retrasado".</returns>
        public async Task<int> ActualizarRetrasadosAsync()
        {
            const string sql = @"
                UPDATE Prestamo SET IDEstadoPrestamo = @Retrasado
                WHERE IDEstadoPrestamo = @Prestado
                  AND CAST(FechaLimite AS DATE) < CAST(GETDATE() AS DATE)";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Retrasado", EstadosPrestamo.Retrasado);
            comando.Parameters.AddWithValue("@Prestado", EstadosPrestamo.Prestado);

            await conexion.OpenAsync();
            return await comando.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Consulta los préstamos con sus libros, filtrando opcionalmente por estado y por usuario.
        /// </summary>
        /// <param name="idEstadoPrestamo">Estado a filtrar, o null para todos.</param>
        /// <param name="textoUsuario">Parte del nombre o del correo del usuario, o null para todos.</param>
        public async Task<List<PrestamoResumen>> ConsultarAsync(int? idEstadoPrestamo, string? textoUsuario)
        {
            var sql = new StringBuilder(@"
                SELECT p.IDPrestamo, p.IDUsuario, u.Nombre + ' ' + u.Apellido, u.Correo, t.NombreTipo,
                       p.FechaPrestamo, p.FechaLimite, p.IDEstadoPrestamo, ep.NombreEstado,
                       (SELECT STRING_AGG(l.Titulo, ', ')
                          FROM DetallePrestamo dp
                          INNER JOIN Ejemplar e ON e.IDEjemplar = dp.IDEjemplar
                          INNER JOIN Libro l ON l.IDLibro = e.IDLibro
                         WHERE dp.IDPrestamo = p.IDPrestamo),
                       (SELECT COUNT(*) FROM DetallePrestamo dp WHERE dp.IDPrestamo = p.IDPrestamo)
                FROM Prestamo p
                INNER JOIN Usuario u ON u.IDUsuario = p.IDUsuario
                INNER JOIN TipoUsuario t ON t.IDTipoUsuario = u.IDTipoUsuario
                INNER JOIN EstadoPrestamo ep ON ep.IDEstadoPrestamo = p.IDEstadoPrestamo
                WHERE 1 = 1");

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand { Connection = conexion };

            // Los filtros se agregan solo si vienen, siempre como parámetros (evita inyección SQL)
            if (idEstadoPrestamo.HasValue)
            {
                sql.Append(" AND p.IDEstadoPrestamo = @IDEstadoPrestamo");
                comando.Parameters.AddWithValue("@IDEstadoPrestamo", idEstadoPrestamo.Value);
            }
            if (!string.IsNullOrWhiteSpace(textoUsuario))
            {
                sql.Append(" AND (u.Nombre + ' ' + u.Apellido LIKE @Texto OR u.Correo LIKE @Texto)");
                comando.Parameters.AddWithValue("@Texto", $"%{textoUsuario.Trim()}%");
            }
            sql.Append(" ORDER BY p.FechaPrestamo DESC, p.IDPrestamo DESC");
            comando.CommandText = sql.ToString();

            var listaPrestamos = new List<PrestamoResumen>();
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaPrestamos.Add(new PrestamoResumen
                {
                    IDPrestamo = lector.GetInt32(0),
                    IDUsuario = lector.GetInt32(1),
                    NombreUsuario = lector.GetString(2),
                    CorreoUsuario = lector.GetString(3),
                    NombreTipoUsuario = lector.GetString(4),
                    FechaPrestamo = lector.GetDateTime(5),
                    FechaLimite = lector.GetDateTime(6),
                    IDEstadoPrestamo = lector.GetInt32(7),
                    NombreEstado = lector.GetString(8),
                    Libros = lector.IsDBNull(9) ? string.Empty : lector.GetString(9),
                    CantidadLibros = lector.GetInt32(10)
                });
            }
            return listaPrestamos;
        }

        /// <summary>
        /// Consulta los ejemplares que se pueden prestar: ejemplar "Disponible" de un libro "Activo" (RN-03).
        /// </summary>
        public async Task<List<EjemplarDisponible>> ConsultarEjemplaresDisponiblesAsync()
        {
            const string sql = @"
                SELECT e.IDEjemplar, e.CodigoInventario, l.Titulo
                FROM Ejemplar e
                INNER JOIN Libro l ON l.IDLibro = e.IDLibro
                WHERE e.IDEstadoEjemplar = @Disponible AND l.IDEstadoLibro = @Activo
                ORDER BY l.Titulo, e.CodigoInventario";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Disponible", EstadosEjemplar.Disponible);
            comando.Parameters.AddWithValue("@Activo", EstadosLibro.Activo);

            var listaEjemplares = new List<EjemplarDisponible>();
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaEjemplares.Add(new EjemplarDisponible
                {
                    IDEjemplar = lector.GetInt32(0),
                    CodigoInventario = lector.GetString(1),
                    Titulo = lector.GetString(2)
                });
            }
            return listaEjemplares;
        }

        /// <summary>
        /// Cuenta cuántos libros tiene prestados un usuario en préstamos vigentes (prestados o retrasados).
        /// Se usa para validar el máximo de libros por rol (RN-01).
        /// </summary>
        public async Task<int> ContarLibrosPrestadosAsync(int idUsuario)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM DetallePrestamo dp
                INNER JOIN Prestamo p ON p.IDPrestamo = dp.IDPrestamo
                WHERE p.IDUsuario = @IDUsuario AND p.IDEstadoPrestamo IN (@Prestado, @Retrasado)";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IDUsuario", idUsuario);
            comando.Parameters.AddWithValue("@Prestado", EstadosPrestamo.Prestado);
            comando.Parameters.AddWithValue("@Retrasado", EstadosPrestamo.Retrasado);

            await conexion.OpenAsync();
            return (int)(await comando.ExecuteScalarAsync())!;
        }

        /// <summary>
        /// Registra un préstamo con sus ejemplares en una sola transacción:
        /// inserta el préstamo, marca cada ejemplar como "Prestado" e inserta su detalle.
        /// </summary>
        /// <exception cref="InvalidOperationException">Si un ejemplar dejó de estar disponible.</exception>
        /// <returns>Identificador del préstamo creado.</returns>
        public async Task<int> RegistrarAsync(Prestamo prestamo, IReadOnlyList<int> idsEjemplares)
        {
            using var conexion = _conexionBd.CrearConexion();
            await conexion.OpenAsync();
            using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();

            try
            {
                // 1. Encabezado del préstamo
                const string sqlPrestamo = @"
                    INSERT INTO Prestamo (IDUsuario, FechaPrestamo, FechaLimite, IDBibliotecario, IDEstadoPrestamo)
                    OUTPUT INSERTED.IDPrestamo
                    VALUES (@IDUsuario, @FechaPrestamo, @FechaLimite, @IDBibliotecario, @IDEstadoPrestamo)";

                int idPrestamo;
                using (var comando = new SqlCommand(sqlPrestamo, conexion, transaccion))
                {
                    comando.Parameters.AddWithValue("@IDUsuario", prestamo.IDUsuario);
                    comando.Parameters.AddWithValue("@FechaPrestamo", prestamo.FechaPrestamo);
                    comando.Parameters.AddWithValue("@FechaLimite", prestamo.FechaLimite);
                    comando.Parameters.AddWithValue("@IDBibliotecario", (object?)prestamo.IDBibliotecario ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@IDEstadoPrestamo", EstadosPrestamo.Prestado);
                    idPrestamo = (int)(await comando.ExecuteScalarAsync())!;
                }

                foreach (var idEjemplar in idsEjemplares)
                {
                    // 2. El ejemplar pasa a "Prestado" solo si sigue "Disponible".
                    //    Así dos bibliotecarios no pueden prestar el mismo ejemplar al mismo tiempo.
                    const string sqlEjemplar = @"
                        UPDATE Ejemplar SET IDEstadoEjemplar = @Prestado
                        WHERE IDEjemplar = @IDEjemplar AND IDEstadoEjemplar = @Disponible";

                    using (var comando = new SqlCommand(sqlEjemplar, conexion, transaccion))
                    {
                        comando.Parameters.AddWithValue("@Prestado", EstadosEjemplar.Prestado);
                        comando.Parameters.AddWithValue("@IDEjemplar", idEjemplar);
                        comando.Parameters.AddWithValue("@Disponible", EstadosEjemplar.Disponible);
                        if (await comando.ExecuteNonQueryAsync() == 0)
                        {
                            throw new InvalidOperationException(
                                "Uno de los ejemplares seleccionados ya no está disponible. Actualice la lista e intente de nuevo.");
                        }
                    }

                    // 3. Detalle del préstamo
                    const string sqlDetalle = "INSERT INTO DetallePrestamo (IDPrestamo, IDEjemplar) VALUES (@IDPrestamo, @IDEjemplar)";
                    using (var comando = new SqlCommand(sqlDetalle, conexion, transaccion))
                    {
                        comando.Parameters.AddWithValue("@IDPrestamo", idPrestamo);
                        comando.Parameters.AddWithValue("@IDEjemplar", idEjemplar);
                        await comando.ExecuteNonQueryAsync();
                    }
                }

                await transaccion.CommitAsync();
                return idPrestamo;
            }
            catch
            {
                await transaccion.RollbackAsync();   // se deshace todo lo hecho en este préstamo
                throw;
            }
        }

        /// <summary>
        /// Registra la devolución de todos los ejemplares de un préstamo en una sola transacción:
        /// cierra el préstamo, inserta una devolución por ejemplar con su multa y libera los ejemplares.
        /// </summary>
        /// <param name="multaPorEjemplar">Multa calculada para cada ejemplar (0 si se devolvió a tiempo).</param>
        /// <exception cref="InvalidOperationException">Si el préstamo ya estaba devuelto o no existe.</exception>
        /// <returns>Cantidad de ejemplares devueltos.</returns>
        public async Task<int> RegistrarDevolucionAsync(int idPrestamo, DateTime fechaDevolucion,
            string? observaciones, decimal multaPorEjemplar)
        {
            using var conexion = _conexionBd.CrearConexion();
            await conexion.OpenAsync();
            using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();

            try
            {
                // 1. Cerrar el préstamo, solo si sigue vigente
                const string sqlPrestamo = @"
                    UPDATE Prestamo SET IDEstadoPrestamo = @Devuelto
                    WHERE IDPrestamo = @IDPrestamo AND IDEstadoPrestamo IN (@Prestado, @Retrasado)";

                using (var comando = new SqlCommand(sqlPrestamo, conexion, transaccion))
                {
                    comando.Parameters.AddWithValue("@Devuelto", EstadosPrestamo.Devuelto);
                    comando.Parameters.AddWithValue("@IDPrestamo", idPrestamo);
                    comando.Parameters.AddWithValue("@Prestado", EstadosPrestamo.Prestado);
                    comando.Parameters.AddWithValue("@Retrasado", EstadosPrestamo.Retrasado);
                    if (await comando.ExecuteNonQueryAsync() == 0)
                    {
                        throw new InvalidOperationException("El préstamo no existe o ya fue devuelto.");
                    }
                }

                // 2. Detalles del préstamo (se leen completos antes de ejecutar otros comandos)
                var listaDetalles = await ConsultarDetallesAsync(idPrestamo, conexion, transaccion);

                foreach (var (idDetallePrestamo, idEjemplar) in listaDetalles)
                {
                    // 3. Una devolución por ejemplar
                    const string sqlDevolucion = @"
                        INSERT INTO Devolucion (FechaDevolucion, Observaciones, Multa, IDDetallePrestamo)
                        VALUES (@FechaDevolucion, @Observaciones, @Multa, @IDDetallePrestamo)";

                    using (var comando = new SqlCommand(sqlDevolucion, conexion, transaccion))
                    {
                        comando.Parameters.AddWithValue("@FechaDevolucion", fechaDevolucion);
                        comando.Parameters.AddWithValue("@Observaciones",
                            string.IsNullOrWhiteSpace(observaciones) ? (object)DBNull.Value : observaciones.Trim());
                        comando.Parameters.AddWithValue("@Multa", multaPorEjemplar);
                        comando.Parameters.AddWithValue("@IDDetallePrestamo", idDetallePrestamo);
                        await comando.ExecuteNonQueryAsync();
                    }

                    // 4. El ejemplar vuelve a estar disponible (RF-006)
                    await CambiarEstadoEjemplarAsync(idEjemplar, EstadosEjemplar.Disponible, conexion, transaccion);
                }

                await transaccion.CommitAsync();
                return listaDetalles.Count;
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Anula un préstamo registrado por error: libera sus ejemplares y borra el detalle y el préstamo.
        /// Solo se permite en préstamos vigentes (sin devolución).
        /// </summary>
        /// <exception cref="InvalidOperationException">Si el préstamo ya fue devuelto o no existe.</exception>
        public async Task AnularAsync(int idPrestamo)
        {
            using var conexion = _conexionBd.CrearConexion();
            await conexion.OpenAsync();
            using var transaccion = (SqlTransaction)await conexion.BeginTransactionAsync();

            try
            {
                const string sqlValidar = @"
                    SELECT COUNT(*) FROM Prestamo
                    WHERE IDPrestamo = @IDPrestamo AND IDEstadoPrestamo IN (@Prestado, @Retrasado)";

                using (var comando = new SqlCommand(sqlValidar, conexion, transaccion))
                {
                    comando.Parameters.AddWithValue("@IDPrestamo", idPrestamo);
                    comando.Parameters.AddWithValue("@Prestado", EstadosPrestamo.Prestado);
                    comando.Parameters.AddWithValue("@Retrasado", EstadosPrestamo.Retrasado);
                    if ((int)(await comando.ExecuteScalarAsync())! == 0)
                    {
                        throw new InvalidOperationException("Solo se pueden anular préstamos que no han sido devueltos.");
                    }
                }

                var listaDetalles = await ConsultarDetallesAsync(idPrestamo, conexion, transaccion);
                foreach (var (_, idEjemplar) in listaDetalles)
                {
                    await CambiarEstadoEjemplarAsync(idEjemplar, EstadosEjemplar.Disponible, conexion, transaccion);
                }

                const string sqlBorrar = @"
                    DELETE FROM DetallePrestamo WHERE IDPrestamo = @IDPrestamo;
                    DELETE FROM Prestamo WHERE IDPrestamo = @IDPrestamo;";

                using (var comando = new SqlCommand(sqlBorrar, conexion, transaccion))
                {
                    comando.Parameters.AddWithValue("@IDPrestamo", idPrestamo);
                    await comando.ExecuteNonQueryAsync();
                }

                await transaccion.CommitAsync();
            }
            catch
            {
                await transaccion.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Consulta el historial de devoluciones, de la más reciente a la más antigua.
        /// </summary>
        /// <param name="textoBusqueda">Parte del nombre del usuario o del título del libro, o null para todas.</param>
        public async Task<List<DevolucionResumen>> ConsultarDevolucionesAsync(string? textoBusqueda)
        {
            var sql = new StringBuilder(@"
                SELECT d.IDDevolucion, p.IDPrestamo, u.Nombre + ' ' + u.Apellido, l.Titulo, e.CodigoInventario,
                       p.FechaLimite, d.FechaDevolucion, d.Multa, d.Observaciones
                FROM Devolucion d
                INNER JOIN DetallePrestamo dp ON dp.IDDetallePrestamo = d.IDDetallePrestamo
                INNER JOIN Prestamo p ON p.IDPrestamo = dp.IDPrestamo
                INNER JOIN Usuario u ON u.IDUsuario = p.IDUsuario
                INNER JOIN Ejemplar e ON e.IDEjemplar = dp.IDEjemplar
                INNER JOIN Libro l ON l.IDLibro = e.IDLibro");

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand { Connection = conexion };

            if (!string.IsNullOrWhiteSpace(textoBusqueda))
            {
                sql.Append(" WHERE u.Nombre + ' ' + u.Apellido LIKE @Texto OR l.Titulo LIKE @Texto");
                comando.Parameters.AddWithValue("@Texto", $"%{textoBusqueda.Trim()}%");
            }
            sql.Append(" ORDER BY d.FechaDevolucion DESC, d.IDDevolucion DESC");
            comando.CommandText = sql.ToString();

            var listaDevoluciones = new List<DevolucionResumen>();
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaDevoluciones.Add(new DevolucionResumen
                {
                    IDDevolucion = lector.GetInt32(0),
                    IDPrestamo = lector.GetInt32(1),
                    NombreUsuario = lector.GetString(2),
                    Titulo = lector.GetString(3),
                    CodigoInventario = lector.GetString(4),
                    FechaLimite = lector.GetDateTime(5),
                    FechaDevolucion = lector.GetDateTime(6),
                    Multa = Convert.ToDecimal(lector.GetValue(7)),
                    Observaciones = lector.IsDBNull(8) ? null : lector.GetString(8)
                });
            }
            return listaDevoluciones;
        }

        // ---------- MÉTODOS PRIVADOS DE APOYO (trabajan dentro de la transacción recibida) ----------

        /// <summary>Lee los pares (IDDetallePrestamo, IDEjemplar) de un préstamo.</summary>
        private static async Task<List<(int IdDetallePrestamo, int IdEjemplar)>> ConsultarDetallesAsync(
            int idPrestamo, SqlConnection conexion, SqlTransaction transaccion)
        {
            const string sql = "SELECT IDDetallePrestamo, IDEjemplar FROM DetallePrestamo WHERE IDPrestamo = @IDPrestamo";

            var listaDetalles = new List<(int, int)>();
            using var comando = new SqlCommand(sql, conexion, transaccion);
            comando.Parameters.AddWithValue("@IDPrestamo", idPrestamo);
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaDetalles.Add((lector.GetInt32(0), lector.GetInt32(1)));
            }
            return listaDetalles;
        }

        /// <summary>Cambia el estado de un ejemplar.</summary>
        private static async Task CambiarEstadoEjemplarAsync(int idEjemplar, int idEstadoEjemplar,
            SqlConnection conexion, SqlTransaction transaccion)
        {
            const string sql = "UPDATE Ejemplar SET IDEstadoEjemplar = @IDEstadoEjemplar WHERE IDEjemplar = @IDEjemplar";

            using var comando = new SqlCommand(sql, conexion, transaccion);
            comando.Parameters.AddWithValue("@IDEstadoEjemplar", idEstadoEjemplar);
            comando.Parameters.AddWithValue("@IDEjemplar", idEjemplar);
            await comando.ExecuteNonQueryAsync();
        }
    }
}
