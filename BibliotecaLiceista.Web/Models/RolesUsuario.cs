namespace BibliotecaLiceista.Web.Models
{
    // Nombres de los roles tal como están en la columna NombreTipo de la tabla TipoUsuario.
    // Si en la base de datos se escriben distinto, solo se cambian aquí.
    public static class RolesUsuario
    {
        public const string Administrador = "Administrador";
        public const string Bibliotecario = "Bibliotecario";
        public const string Alumno = "Alumno";
        public const string Docente = "Docente";

        public const string AlumnoYDocente = Alumno + "," + Docente;
    }
}
