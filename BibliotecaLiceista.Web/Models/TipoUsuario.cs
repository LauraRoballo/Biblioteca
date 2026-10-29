namespace BibliotecaLiceista.Web.Models
{
    public class TipoUsuario
    {
        public int IDTipoUsuario { get; set; }
        public string NombreTipo { get; set; } = string.Empty;
        public int MaxLibros { get; set; }
    }
}
