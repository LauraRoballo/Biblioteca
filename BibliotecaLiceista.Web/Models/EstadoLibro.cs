namespace BibliotecaLiceista.Web.Models
{
    public class EstadoLibro
    {
        public int IDEstadoLibro { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
