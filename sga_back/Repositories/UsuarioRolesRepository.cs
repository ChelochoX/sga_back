using Dapper;
using sga_back.Repositories.Interfaces;
using System.Data;

namespace sga_back.Repositories;

public class UsuarioRolesRepository : IUsuarioRolesRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<UsuarioRolesRepository> _logger;

    public UsuarioRolesRepository(ILogger<UsuarioRolesRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<bool> AsignarRol(int idUsuario, int idRol)
    {
        try
        {
            string query = "INSERT INTO Usuario_Roles (id_usuario, id_rol) VALUES (@IdUsuario, @IdRol)";
            int filasAfectadas = await _conexion.ExecuteAsync(query, new { IdUsuario = idUsuario, IdRol = idRol });

            _logger.LogInformation("Rol con ID: {IdRol} asignado al usuario con ID: {IdUsuario}", idRol, idUsuario);
            return filasAfectadas > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar rol al usuario.");
            throw;
        }
    }

    public async Task<bool> EliminarRol(int idUsuario, int idRol)
    {
        try
        {
            string query = "DELETE FROM Usuario_Roles WHERE id_usuario = @IdUsuario AND id_rol = @IdRol";
            int filasAfectadas = await _conexion.ExecuteAsync(query, new { IdUsuario = idUsuario, IdRol = idRol });

            _logger.LogInformation("Rol con ID: {IdRol} eliminado del usuario con ID: {IdUsuario}", idRol, idUsuario);
            return filasAfectadas > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar rol del usuario.");
            throw;
        }
    }

    public async Task<IEnumerable<int>> ObtenerRolesPorUsuario(int idUsuario)
    {
        try
        {
            string query = "SELECT id_rol FROM Usuario_Roles WHERE id_usuario = @IdUsuario";
            return await _conexion.QueryAsync<int>(query, new { IdUsuario = idUsuario });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles del usuario.");
            throw;
        }
    }
}
