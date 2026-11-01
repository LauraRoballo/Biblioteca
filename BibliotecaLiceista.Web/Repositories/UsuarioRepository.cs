using BibliotecaLiceista.Web.Data;
using BibliotecaLiceista.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;

namespace BibliotecaLiceista.Web.Repositories
{
    public class UsuarioRepository
    {
        private readonly ConexionBd _conexionBd;
        private readonly PasswordHasher<Usuario> _cifradorContrasenia = new();

        private const string ConsultaBase = @"
            SELECT u.IDUsuario, u.Nombre, u.Apellido, u.Correo, u.Telefono,
                   u.FechaRegistro, u.IDTipoUsuario, t.NombreTipo, u.Contrasenia
            FROM Usuario u
            INNER JOIN TipoUsuario t ON t.IDTipoUsuario = u.IDTipoUsuario";

        public UsuarioRepository(ConexionBd conexionBd)
        {
            _conexionBd = conexionBd;
        }

        // ---------- INSERTAR (Registro) ----------
        public async Task<int> InsertarAsync(Usuario usuario)
        {
            const string sql = @"
                INSERT INTO Usuario (Nombre, Apellido, Correo, Telefono, FechaRegistro, IDTipoUsuario, Contrasenia)
                OUTPUT INSERTED.IDUsuario
                VALUES (@Nombre, @Apellido, @Correo, @Telefono, @FechaRegistro, @IDTipoUsuario, @Contrasenia)";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            comando.Parameters.AddWithValue("@Apellido", usuario.Apellido);
            comando.Parameters.AddWithValue("@Correo", usuario.Correo);
            comando.Parameters.AddWithValue("@Telefono", (object?)usuario.Telefono ?? DBNull.Value);
            comando.Parameters.AddWithValue("@FechaRegistro", DateTime.Now);
            comando.Parameters.AddWithValue("@IDTipoUsuario", usuario.IDTipoUsuario);
            comando.Parameters.AddWithValue("@Contrasenia",
                _cifradorContrasenia.HashPassword(usuario, usuario.Contrasenia));   // se guarda cifrada

            await conexion.OpenAsync();
            return (int)(await comando.ExecuteScalarAsync())!;
        }

        // ---------- CONSULTAR TODOS ----------
        public async Task<List<Usuario>> ConsultarTodosAsync()
        {
            string sql = ConsultaBase + " ORDER BY u.Nombre, u.Apellido";
            var listaUsuarios = new List<Usuario>();

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaUsuarios.Add(MapearUsuario(lector));
            }
            return listaUsuarios;
        }

        // ---------- CONSULTAR POR CORREO (Login y validar duplicados) ----------
        public async Task<Usuario?> ConsultarPorCorreoAsync(string correo)
        {
            string sql = ConsultaBase + " WHERE u.Correo = @Correo";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Correo", correo);
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            return await lector.ReadAsync() ? MapearUsuario(lector) : null;
        }

        // ---------- CONSULTAR POR ID (validaciones del módulo de préstamos) ----------
        /// <summary>Consulta un usuario por su identificador, con el nombre de su rol.</summary>
        public async Task<Usuario?> ConsultarPorIdAsync(int idUsuario)
        {
            string sql = ConsultaBase + " WHERE u.IDUsuario = @IDUsuario";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IDUsuario", idUsuario);
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            return await lector.ReadAsync() ? MapearUsuario(lector) : null;
        }

        // ---------- ACTUALIZAR (no cambia la contraseña) ----------
        public async Task<bool> ActualizarAsync(Usuario usuario)
        {
            const string sql = @"
                UPDATE Usuario SET Nombre = @Nombre, Apellido = @Apellido, Correo = @Correo,
                                   Telefono = @Telefono, IDTipoUsuario = @IDTipoUsuario
                WHERE IDUsuario = @IDUsuario";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IDUsuario", usuario.IDUsuario);
            comando.Parameters.AddWithValue("@Nombre", usuario.Nombre);
            comando.Parameters.AddWithValue("@Apellido", usuario.Apellido);
            comando.Parameters.AddWithValue("@Correo", usuario.Correo);
            comando.Parameters.AddWithValue("@Telefono", (object?)usuario.Telefono ?? DBNull.Value);
            comando.Parameters.AddWithValue("@IDTipoUsuario", usuario.IDTipoUsuario);

            await conexion.OpenAsync();
            return await comando.ExecuteNonQueryAsync() > 0;
        }

        // ---------- ELIMINAR ----------
        public async Task<bool> EliminarAsync(int idUsuario)
        {
            const string sql = "DELETE FROM Usuario WHERE IDUsuario = @IDUsuario";

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@IDUsuario", idUsuario);
            await conexion.OpenAsync();
            return await comando.ExecuteNonQueryAsync() > 0;
        }

        // ---------- TIPOS DE USUARIO (para las listas desplegables) ----------
        public async Task<List<TipoUsuario>> ConsultarTiposUsuarioAsync()
        {
            const string sql = "SELECT IDTipoUsuario, NombreTipo, MaxLibros FROM TipoUsuario ORDER BY NombreTipo";
            var listaTipos = new List<TipoUsuario>();

            using var conexion = _conexionBd.CrearConexion();
            using var comando = new SqlCommand(sql, conexion);
            await conexion.OpenAsync();
            using var lector = await comando.ExecuteReaderAsync();
            while (await lector.ReadAsync())
            {
                listaTipos.Add(new TipoUsuario
                {
                    IDTipoUsuario = lector.GetInt32(0),
                    NombreTipo = lector.GetString(1),
                    MaxLibros = lector.GetInt32(2)
                });
            }
            return listaTipos;
        }

        // ---------- VERIFICAR CONTRASEÑA (Login) ----------
        public bool VerificarContrasenia(Usuario usuario, string contraseniaIngresada)
        {
            var resultado = _cifradorContrasenia.VerifyHashedPassword(
                usuario, usuario.Contrasenia, contraseniaIngresada);
            return resultado != PasswordVerificationResult.Failed;
        }

        private static Usuario MapearUsuario(SqlDataReader lector)
        {
            return new Usuario
            {
                IDUsuario = lector.GetInt32(0),
                Nombre = lector.GetString(1),
                Apellido = lector.GetString(2),
                Correo = lector.GetString(3),
                Telefono = lector.IsDBNull(4) ? null : lector.GetString(4),
                FechaRegistro = lector.GetDateTime(5),
                IDTipoUsuario = lector.GetInt32(6),
                NombreTipoUsuario = lector.GetString(7),
                Contrasenia = lector.GetString(8)
            };
        }
    }
}