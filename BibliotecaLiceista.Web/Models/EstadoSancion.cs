namespace BibliotecaLiceista.Web.Models
{
    public class EstadoSancion
    {
        public int IDEstadoSancion { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
