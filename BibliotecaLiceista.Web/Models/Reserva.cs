namespace BibliotecaLiceista.Web.Models
{
    public class Reserva
    {
        public int IDReserva { get; set; }
        public int IDUsuario { get; set; }
        public int IDLibro { get; set; }
        public DateTime FechaReserva { get; set; }
        public int IDEstadoReserva { get; set; }
    }
}
