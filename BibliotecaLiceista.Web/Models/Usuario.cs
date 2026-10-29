namespace BibliotecaLiceista.Web.Models
{
    public class Usuario
    {
        public int IDUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int IDTipoUsuario { get; set; }
    }
}
