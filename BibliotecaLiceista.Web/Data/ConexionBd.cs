using Microsoft.Data.SqlClient;

namespace BibliotecaLiceista.Web.Data
{
    public class ConexionBd
    {
        private readonly string _cadenaConexion;

        public ConexionBd(IConfiguration configuracion)
        {
            _cadenaConexion = configuracion.GetConnectionString("BibliotecaDB")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'BibliotecaDB'.");
        }

        public SqlConnection CrearConexion()
        {
            return new SqlConnection(_cadenaConexion);
        }
    }
}