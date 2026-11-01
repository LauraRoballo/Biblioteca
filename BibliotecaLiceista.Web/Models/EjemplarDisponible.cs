namespace BibliotecaLiceista.Web.Models
{
    /// <summary>
    /// Ejemplar que se puede prestar: su estado es "Disponible" y su libro está "Activo".
    /// </summary>
    public class EjemplarDisponible
    {
        public int IDEjemplar { get; set; }
        public string CodigoInventario { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
    }
}
