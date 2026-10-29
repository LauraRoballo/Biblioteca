namespace BibliotecaLiceista.Web.Models
{
    public class Libro
    {
        public int IDLibro { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? ISBN { get; set; }
        public string? Edicion { get; set; }
        public int? AnioPublicacion { get; set; }
        public string? Descripcion { get; set; }
        public int IDEditorial { get; set; }
        public int IDCategoria { get; set; }
        public int IDEstadoLibro { get; set; }
    }
}
