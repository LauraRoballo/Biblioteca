using System.ComponentModel.DataAnnotations;

namespace BibliotecaLiceista.Web.Models
{
    public class Libro
    {
        public int IDLibro { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, ErrorMessage = "El título no puede superar 200 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        public string? ISBN { get; set; }
        public string? Edicion { get; set; }

        [Range(1000, 2100, ErrorMessage = "Ingrese un año válido.")]
        public int? AnioPublicacion { get; set; }

        public string? Descripcion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione la editorial.")]
        public int IDEditorial { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione la categoría.")]
        public int IDCategoria { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione el estado.")]
        public int IDEstadoLibro { get; set; }

        // No son columnas de la tabla Libro: se llenan con los JOIN para mostrar los nombres
        public string? NombreEditorial { get; set; }
        public string? NombreCategoria { get; set; }
        public string? NombreEstadoLibro { get; set; }
    }
}
