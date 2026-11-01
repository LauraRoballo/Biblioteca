namespace BibliotecaLiceista.Web.Models
{
    /// <summary>
    /// Datos de una devolución listos para mostrar en el historial de devoluciones.
    /// </summary>
    public class DevolucionResumen
    {
        public int IDDevolucion { get; set; }
        public int IDPrestamo { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string CodigoInventario { get; set; } = string.Empty;
        public DateTime FechaLimite { get; set; }
        public DateTime FechaDevolucion { get; set; }
        public decimal Multa { get; set; }
        public string? Observaciones { get; set; }

        /// <summary>Días entre la fecha límite y la fecha de devolución (0 si se devolvió a tiempo).</summary>
        public int DiasRetraso => Math.Max(0, (FechaDevolucion.Date - FechaLimite.Date).Days);
    }
}
