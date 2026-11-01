namespace BibliotecaLiceista.Web.Models
{
    /// <summary>
    /// Identificadores de la tabla EstadoEjemplar que usa el módulo de préstamos.
    /// </summary>
    public static class EstadosEjemplar
    {
        /// <summary>El ejemplar está en la biblioteca y se puede prestar.</summary>
        public const int Disponible = 1;

        /// <summary>El ejemplar está en manos de un usuario.</summary>
        public const int Prestado = 2;
    }
}
