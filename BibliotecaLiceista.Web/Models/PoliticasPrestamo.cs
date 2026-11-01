namespace BibliotecaLiceista.Web.Models
{
    /// <summary>
    /// Valores de las reglas de negocio de préstamo.
    /// Se leen de la sección "PoliticasPrestamo" de appsettings.json,
    /// así se pueden cambiar sin modificar el código.
    /// </summary>
    public class PoliticasPrestamo
    {
        /// <summary>Días máximos de préstamo para un alumno (RN-02).</summary>
        public int DiasAlumno { get; set; } = 8;

        /// <summary>Días máximos de préstamo para un docente (RN-02).</summary>
        public int DiasDocente { get; set; } = 15;

        /// <summary>Valor en pesos que se cobra por cada día de retraso y por cada ejemplar.</summary>
        public decimal MultaPorDia { get; set; } = 1000;
    }
}
