namespace BibliotecaLiceista.Web.Models
{
    public class EstadoReserva
    {
        public int IDEstadoReserva { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
