namespace BibliotecaLiceista.Web.Models
{
    public class Sancion
    {
        public int IDSancion { get; set; }
        public int IDPrestamo { get; set; }
        public decimal Valor { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public int IDEstadoSancion { get; set; }
    }
}
