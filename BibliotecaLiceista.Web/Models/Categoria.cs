namespace BibliotecaLiceista.Web.Models
{
    public class Categoria
    {
        public int IDCategoria { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
