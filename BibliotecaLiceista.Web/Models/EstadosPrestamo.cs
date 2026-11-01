namespace BibliotecaLiceista.Web.Models
{
    /// <summary>
    /// Identificadores de la tabla EstadoPrestamo.
    /// Se usan como constantes para no escribir números "mágicos" en las consultas.
    /// </summary>
    public static class EstadosPrestamo
    {
        /// <summary>Préstamo vigente, dentro de la fecha límite.</summary>
        public const int Prestado = 1;

        /// <summary>Préstamo cerrado: todos los ejemplares fueron devueltos.</summary>
        public const int Devuelto = 2;

        /// <summary>Préstamo vigente cuya fecha límite ya pasó.</summary>
        public const int Retrasado = 3;
    }
}
