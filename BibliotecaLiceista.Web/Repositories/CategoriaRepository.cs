using BibliotecaLiceista.Web.Data;
using BibliotecaLiceista.Web.Models;
using Microsoft.Data.SqlClient;

namespace BibliotecaLiceista.Web.Repositories
{
    public class CategoriaRepository
    {
        private readonly ConexionBd _conexionBd;

        public CategoriaRepository(ConexionBd conexionBd)
        {
            _conexionBd = conexionBd;
        }

        // ---------- CONSULTAR TODAS (lista desplegable del formulario de libros) ----------
        public async Task<List<Categoria>> ConsultarTodosAsync()
        {
            const string sql = "SELECT IDCategoria, NombreCategoria, Descripcion FROM Categoria ORDER BY NombreCategoria";
            var listaCategorias = new List<Categoria>();

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaCategorias.Add(new Categoria
                {
                    IDCategoria = lector.GetInt32(0),
                    NombreCategoria = lector.GetString(1),
                    Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2)
                });
            }
            return listaCategorias;
        }
    }
}
