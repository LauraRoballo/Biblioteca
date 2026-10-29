namespace BibliotecaLiceista.Web.Models
{
    public class Autor
    {
        public int IDAutor { get; set; }
        public string NombreAutor { get; set; } = string.Empty;
        public string? Nacionalidad { get; set; }
        public DateTime? FechaNacimiento { get; set; }
    }
}
