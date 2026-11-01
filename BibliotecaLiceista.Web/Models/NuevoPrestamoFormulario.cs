using System.ComponentModel.DataAnnotations;

namespace BibliotecaLiceista.Web.Models
{
    /// <summary>
    /// Datos que el bibliotecario diligencia en el formulario "Registrar préstamo".
    /// Las anotaciones validan el formato; las reglas de negocio se validan en PrestamoService.
    /// </summary>
    public class NuevoPrestamoFormulario
    {
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione el usuario que recibe el préstamo.")]
        public int IDUsuario { get; set; }

        [Required(ErrorMessage = "Ingrese la fecha límite de devolución.")]
        public DateTime? FechaLimite { get; set; }

        /// <summary>Ejemplares marcados en el formulario.</summary>
        public List<int> IDsEjemplares { get; set; } = new();
    }
}
