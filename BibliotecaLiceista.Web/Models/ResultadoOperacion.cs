namespace BibliotecaLiceista.Web.Models
{
    /// <summary>
    /// Resultado de una operación de negocio: el mensaje para el usuario
    /// y la lista de errores de validación, si los hubo.
    /// </summary>
    public class ResultadoOperacion
    {
        /// <summary>Es verdadero cuando la operación no tuvo errores.</summary>
        public bool Exitoso => Errores.Count == 0;

        /// <summary>Mensaje que se muestra al usuario cuando la operación termina bien.</summary>
        public string Mensaje { get; set; } = string.Empty;

        /// <summary>Errores de validación o de reglas de negocio.</summary>
        public List<string> Errores { get; } = new();
    }
}
