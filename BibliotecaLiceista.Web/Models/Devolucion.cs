namespace BibliotecaLiceista.Web.Models
{
    public class Devolucion
    {
        public int IDDevolucion { get; set; }
        public DateTime FechaDevolucion { get; set; }
        public string? Observaciones { get; set; }
        public decimal Multa { get; set; }
        public int IDDetallePrestamo { get; set; }
    }
}
