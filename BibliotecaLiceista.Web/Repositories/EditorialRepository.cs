using BibliotecaLiceista.Web.Data;
using BibliotecaLiceista.Web.Models;
using Microsoft.Data.SqlClient;

namespace BibliotecaLiceista.Web.Repositories
{
    public class EditorialRepository
    {
        private readonly ConexionBd _conexionBd;

        public EditorialRepository(ConexionBd conexionBd)
        {
            _conexionBd = conexionBd;
        }

        // ---------- CONSULTAR TODAS (lista desplegable del formulario de libros) ----------
        public async Task<List<Editorial>> ConsultarTodosAsync()
        {
            const string sql = "SELECT IDEditorial, NombreEditorial FROM Editorial ORDER BY NombreEditorial";
            var listaEditoriales = new List<Editorial>();

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaEditoriales.Add(new Editorial
                {
                    IDEditorial = lector.GetInt32(0),
                    NombreEditorial = lector.GetString(1)
                });
            }
            return listaEditoriales;
        }
    }
}
