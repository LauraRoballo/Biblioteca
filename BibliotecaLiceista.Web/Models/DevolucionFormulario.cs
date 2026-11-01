using System.ComponentModel.DataAnnotations;

namespace BibliotecaLiceista.Web.Models
{
    /// <summary>
    /// Datos que el bibliotecario diligencia en el formulario "Registrar devolución".
    /// </summary>
    public class DevolucionFormulario
    {
        [Required(ErrorMessage = "Ingrese la fecha de devolución.")]
        public DateTime? FechaDevolucion { get; set; }

        [StringLength(200, ErrorMessage = "Las observaciones admiten máximo 200 caracteres.")]
        [RegularExpression(@"^[\p{L}\p{N}\s.,;:()¿?¡!\-]*$",
            ErrorMessage = "Las observaciones solo admiten letras, números y signos de puntuación básicos (. , ; : ( ) ¿ ? ¡ ! -).")]
        public string? Observaciones { get; set; }
    }
}
