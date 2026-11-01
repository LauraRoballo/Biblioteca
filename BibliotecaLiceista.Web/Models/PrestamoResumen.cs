namespace BibliotecaLiceista.Web.Models
{
    /// <summary>
    /// Datos de un préstamo listos para mostrar en pantalla.
    /// Se arma con una consulta que une Prestamo, Usuario, TipoUsuario, EstadoPrestamo y los libros prestados.
    /// </summary>
    public class PrestamoResumen
    {
        public int IDPrestamo { get; set; }
        public int IDUsuario { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoUsuario { get; set; } = string.Empty;
        public string NombreTipoUsuario { get; set; } = string.Empty;
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaLimite { get; set; }
        public int IDEstadoPrestamo { get; set; }
        public string NombreEstado { get; set; } = string.Empty;

        /// <summary>Títulos de los libros prestados, separados por coma.</summary>
        public string Libros { get; set; } = string.Empty;

        public int CantidadLibros { get; set; }

        /// <summary>Un préstamo está activo mientras no se haya devuelto.</summary>
        public bool EstaActivo =>
            IDEstadoPrestamo == EstadosPrestamo.Prestado || IDEstadoPrestamo == EstadosPrestamo.Retrasado;
    }
}
