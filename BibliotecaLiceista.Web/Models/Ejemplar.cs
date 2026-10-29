namespace BibliotecaLiceista.Web.Models
{
    public class Ejemplar
    {
        public int IDEjemplar { get; set; }
        public int IDLibro { get; set; }
        public string CodigoInventario { get; set; } = string.Empty;
        public int IDEstadoEjemplar { get; set; }
    }
}
