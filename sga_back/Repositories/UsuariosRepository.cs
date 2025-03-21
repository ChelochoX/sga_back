using Dapper;
using Microsoft.AspNetCore.Identity;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using System.Data;

namespace sga_back.Repositories;

public class UsuariosRepository : IUsuariosRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<UsuariosRepository> _logger;
    private readonly PasswordHasher<string> _passwordHasher;

    public UsuariosRepository(ILogger<UsuariosRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
        _passwordHasher = new PasswordHasher<string>();
    }

    public async Task<int> Insertar(Usuario usuario)
    {
        try
        {
            _logger.LogInformation("Intentando insertar usuario: {NombreUsuario}", usuario.NombreUsuario);

            // Verificar si el nombre de usuario ya existe
            string queryVerificar = "SELECT COUNT(*) FROM Usuarios WHERE nombre_usuario = @NombreUsuario";
            int existe = await _conexion.ExecuteScalarAsync<int>(queryVerificar, new { usuario.NombreUsuario });

            if (existe > 0)
            {
                _logger.LogWarning("El nombre de usuario {NombreUsuario} ya está registrado.", usuario.NombreUsuario);
                throw new ReglasdeNegocioException("El nombre de usuario ya está registrado.");
            }

            usuario.ContrasenaHash = _passwordHasher.HashPassword(null, usuario.ContrasenaHash);

            // Insertar el nuevo usuario
            string queryInsertar = @"
                INSERT INTO Usuarios (id_persona, nombre_usuario, contrasena_hash, estado, fecha_creacion)
                VALUES (@IdPersona, @NombreUsuario, @ContrasenaHash, @Estado, @FechaCreacion);
                SELECT CAST(SCOPE_IDENTITY() as int);";

            usuario.FechaCreacion = DateTime.UtcNow;
            int id = await _conexion.ExecuteScalarAsync<int>(queryInsertar, usuario);
            _logger.LogInformation("Usuario insertado con ID: {IdUsuario}", id);

            return id;
        }
        catch (ReglasdeNegocioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar usuario");
            throw new RepositoryException("Ocurrió un error al intentar insertar el usuario.", ex);
        }
    }

    public async Task<bool> Eliminar(int id)
    {
        try
        {
            _logger.LogInformation("Intentando eliminar usuario con ID: {Id}", id);
            string query = "DELETE FROM Usuarios WHERE id_usuario = @Id";
            int filasAfectadas = await _conexion.ExecuteAsync(query, new { Id = id });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el usuario con ID: {Id} para eliminar.", id);
                return false;
            }

            _logger.LogInformation("Usuario con ID: {Id} eliminado exitosamente.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar usuario con ID: {Id}", id);
            throw new RepositoryException("Ocurrió un error al intentar eliminar el usuario.", ex);
        }
    }

    public async Task<bool> ExisteNombreUsuario(string nombreUsuario)
    {
        string query = "SELECT COUNT(*) FROM Usuarios WHERE nombre_usuario = @NombreUsuario";
        int count = await _conexion.QuerySingleAsync<int>(query, new { NombreUsuario = nombreUsuario });
        return count > 0;
    }

    public async Task<bool> ActualizarUsuario(int idUsuario, string nombreUsuario, string nuevaContrasena)
    {
        try
        {
            _logger.LogInformation("Intentando actualizar usuario con ID: {IdUsuario}", idUsuario);

            string query = @"
            UPDATE Usuarios
            SET nombre_usuario = @NombreUsuario, 
                contrasena_hash = @ContrasenaHash, 
                estado = 'Activo', 
                fecha_modificacion = GETDATE()
            WHERE id_usuario = @IdUsuario";

            int filasAfectadas = await _conexion.ExecuteAsync(query, new
            {
                IdUsuario = idUsuario,
                NombreUsuario = nombreUsuario,
                ContrasenaHash = nuevaContrasena
            });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el usuario con ID: {IdUsuario} para actualizar.", idUsuario);
                return false;
            }

            _logger.LogInformation("Usuario con ID: {IdUsuario} actualizado exitosamente.", idUsuario);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario con ID: {IdUsuario}", idUsuario);
            throw new RepositoryException("Ocurrió un error al intentar actualizar el usuario.", ex);
        }
    }

    public async Task<Usuario?> ValidarCredenciales(string usuario, string contrasena)
    {
        string query = @"
            SELECT u.id_usuario, u.nombre_usuario, u.contrasena_hash, ur.id_rol
            FROM Usuarios u
            INNER JOIN Usuario_Roles ur ON u.id_usuario = ur.id_usuario
            WHERE u.nombre_usuario = @NombreUsuario";

        var usuarioDb = await _conexion.QueryFirstOrDefaultAsync<Usuario>(query, new { NombreUsuario = usuario });

        if (usuarioDb == null) return null; // Usuario no encontrado

        // 🔐 Verificación del hash de la contraseña
        var resultado = _passwordHasher.VerifyHashedPassword(null, usuarioDb.ContrasenaHash, contrasena);

        if (resultado == PasswordVerificationResult.Success)
        {
            return usuarioDb;
        }

        return null; // Contraseña incorrecta
    }

}
