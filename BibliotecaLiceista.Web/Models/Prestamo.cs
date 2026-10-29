namespace BibliotecaLiceista.Web.Models
{
    public class Prestamo
    {
        public int IDPrestamo { get; set; }
        public int IDUsuario { get; set; }
        public DateTime FechaPrestamo { get; set; }
        public DateTime FechaLimite { get; set; }
        public int? IDBibliotecario { get; set; }
        public int IDEstadoPrestamo { get; set; }
    }
}
