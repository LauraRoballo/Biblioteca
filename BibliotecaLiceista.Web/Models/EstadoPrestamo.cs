namespace BibliotecaLiceista.Web.Models
{
    public class EstadoPrestamo
    {
        public int IDEstadoPrestamo { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
