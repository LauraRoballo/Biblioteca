namespace BibliotecaLiceista.Web.Models
{
    public class Notificacion
    {
        public int IDNotificacion { get; set; }
        public int IDUsuario { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool? Leida { get; set; }
    }
}
