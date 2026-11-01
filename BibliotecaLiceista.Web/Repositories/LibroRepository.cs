using BibliotecaLiceista.Web.Data;
using BibliotecaLiceista.Web.Models;
using Microsoft.Data.SqlClient;

namespace BibliotecaLiceista.Web.Repositories
{
    public class LibroRepository
    {
        private readonly ConexionBd _conexionBd;

        private const string ConsultaBase = @"
            SELECT l.IDLibro, l.Titulo, l.ISBN, l.Edicion, l.AnioPublicacion, l.Descripcion,
                   l.IDEditorial, l.IDCategoria, l.IDEstadoLibro,
                   e.NombreEditorial, c.NombreCategoria, el.NombreEstado
            FROM Libro l
            INNER JOIN Editorial e ON e.IDEditorial = l.IDEditorial
            INNER JOIN Categoria c ON c.IDCategoria = l.IDCategoria
            INNER JOIN EstadoLibro el ON el.IDEstadoLibro = l.IDEstadoLibro";

        public LibroRepository(ConexionBd conexionBd)
        {
            _conexionBd = conexionBd;
        }

        // ---------- INSERTAR ----------
        public async Task<int> InsertarAsync(Libro libro)
        {
            const string sql = @"
                INSERT INTO Libro (Titulo, ISBN, Edicion, AnioPublicacion, Descripcion, IDEditorial, IDCategoria, IDEstadoLibro)
                OUTPUT INSERTED.IDLibro
                VALUES (@Titulo, @ISBN, @Edicion, @AnioPublicacion, @Descripcion, @IDEditorial, @IDCategoria, @IDEstadoLibro)";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            AgregarParametros(comando, libro);

            await conexion.OpenAsync();
            return (int)(await comando.ExecuteScalarAsync())!;   // devuelve el ID generado
        }

        // ---------- CONSULTAR TODOS ----------
        public async Task<List<Libro>> ConsultarTodosAsync()
        {
            string sql = ConsultaBase + " ORDER BY l.Titulo";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            return await LeerListaAsync(conexion, comando);
        }

        // ---------- CONSULTAR POR TÍTULO O ISBN (búsqueda) ----------
        public async Task<List<Libro>> ConsultarPorTituloAsync(string textoBusqueda)
        {
            string sql = ConsultaBase + " WHERE l.Titulo LIKE @Texto OR l.ISBN LIKE @Texto ORDER BY l.Titulo";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Texto", $"%{textoBusqueda}%");
            return await LeerListaAsync(conexion, comando);
        }

        // ---------- CONSULTAR POR ID ----------
        public async Task<Libro?> ConsultarPorIdAsync(int idLibro)
        {
            string sql = ConsultaBase + " WHERE l.IDLibro = @IDLibro";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IDLibro", idLibro);

            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            return await lector.ReadAsync() ? MapearLibro(lector) : null;
        }

        // ---------- ACTUALIZAR ----------
        public async Task<bool> ActualizarAsync(Libro libro)
        {
            const string sql = @"
                UPDATE Libro SET
                    Titulo = @Titulo, ISBN = @ISBN, Edicion = @Edicion,
                    AnioPublicacion = @AnioPublicacion, Descripcion = @Descripcion,
                    IDEditorial = @IDEditorial, IDCategoria = @IDCategoria, IDEstadoLibro = @IDEstadoLibro
                WHERE IDLibro = @IDLibro";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            AgregarParametros(comando, libro);
            comando.Parameters.AddWithValue("@IDLibro", libro.IDLibro);

            await conexion.OpenAsync();
            return await comando.ExecuteNonQueryAsync() > 0;
        }

        // ---------- ELIMINAR ----------
        public async Task<bool> EliminarAsync(int idLibro)
        {
            const string sql = "DELETE FROM Libro WHERE IDLibro = @IDLibro";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IDLibro", idLibro);

            await conexion.OpenAsync();
            return await comando.ExecuteNonQueryAsync() > 0;
        }

        // ---------- ESTADOS DEL LIBRO (lista desplegable) ----------
        public async Task<List<EstadoLibro>> ConsultarEstadosLibroAsync()
        {
            const string sql = "SELECT IDEstadoLibro, NombreEstado, Descripcion FROM EstadoLibro ORDER BY NombreEstado";
            var listaEstados = new List<EstadoLibro>();

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaEstados.Add(new EstadoLibro
                {
                    IDEstadoLibro = lector.GetInt32(0),
                    NombreEstado = lector.GetString(1),
                    Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2)
                });
            }
            return listaEstados;
        }

        // ---------- MÉTODOS PRIVADOS DE APOYO ----------
        private static async Task<List<Libro>> LeerListaAsync(SqlConnection conexion, SqlCommand comando)
        {
            var listaLibros = new List<Libro>();
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaLibros.Add(MapearLibro(lector));
            }
            return listaLibros;
        }

        private static void AgregarParametros(SqlCommand comando, Libro libro)
        {
            comando.Parameters.AddWithValue("@Titulo", libro.Titulo);
            comando.Parameters.AddWithValue("@ISBN", (object?)libro.ISBN ?? DBNull.Value);
            comando.Parameters.AddWithValue("@Edicion", (object?)libro.Edicion ?? DBNull.Value);
            comando.Parameters.AddWithValue("@AnioPublicacion", (object?)libro.AnioPublicacion ?? DBNull.Value);
            comando.Parameters.AddWithValue("@Descripcion", (object?)libro.Descripcion ?? DBNull.Value);
            comando.Parameters.AddWithValue("@IDEditorial", libro.IDEditorial);
            comando.Parameters.AddWithValue("@IDCategoria", libro.IDCategoria);
            comando.Parameters.AddWithValue("@IDEstadoLibro", libro.IDEstadoLibro);
        }

        private static Libro MapearLibro(SqlDataReader lector)
        {
            return new Libro
            {
                IDLibro = lector.GetInt32(0),
                Titulo = lector.GetString(1),
                ISBN = lector.IsDBNull(2) ? null : lector.GetString(2),
                Edicion = lector.IsDBNull(3) ? null : lector.GetString(3),
                AnioPublicacion = lector.IsDBNull(4) ? null : Convert.ToInt32(lector.GetValue(4)),
                Descripcion = lector.IsDBNull(5) ? null : lector.GetString(5),
                IDEditorial = lector.GetInt32(6),
                IDCategoria = lector.GetInt32(7),
                IDEstadoLibro = lector.GetInt32(8),
                NombreEditorial = lector.GetString(9),
                NombreCategoria = lector.GetString(10),
                NombreEstadoLibro = lector.GetString(11)
            };
        }
    }
}
