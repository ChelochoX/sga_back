using Dapper;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using System.Data;

namespace sga_back.Repositories;

public class RolesRepository : IRolesRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<RolesRepository> _logger;

    public RolesRepository(ILogger<RolesRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<int> Insertar(Role role)
    {
        try
        {
            _logger.LogInformation("Intentando insertar rol: {NombreRol}", role.NombreRol);

            if (await ExisteNombreRol(role.NombreRol))
            {
                throw new ReglasdeNegocioException("El nombre del rol ya está registrado.");
            }

            string query = "INSERT INTO Roles (nombre_rol) VALUES (@NombreRol); SELECT CAST(SCOPE_IDENTITY() as int);";
            int id = await _conexion.ExecuteScalarAsync<int>(query, role);

            _logger.LogInformation("Rol insertado con ID: {IdRol}", id);
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar rol.");
            throw new RepositoryException("Ocurrió un error al intentar insertar el rol.", ex);
        }
    }

    public async Task<int> Actualizar(Role role)
    {
        try
        {
            _logger.LogInformation("Intentando actualizar rol con ID: {IdRol}", role.IdRol);

            string query = "UPDATE Roles SET nombre_rol = @NombreRol WHERE id_rol = @IdRol";
            int filasAfectadas = await _conexion.ExecuteAsync(query, role);

            if (filasAfectadas == 0)
            {
                throw new NoDataFoundException("No se encontró el rol para actualizar.");
            }

            _logger.LogInformation("Rol con ID: {IdRol} actualizado exitosamente.", role.IdRol);
            return filasAfectadas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar rol con ID: {IdRol}", role.IdRol);
            throw new RepositoryException("Ocurrió un error al intentar actualizar el rol.", ex);
        }
    }

    public async Task<bool> Eliminar(int id)
    {
        try
        {
            _logger.LogInformation("Intentando eliminar rol con ID: {IdRol}", id);

            string query = "DELETE FROM Roles WHERE id_rol = @Id";
            int filasAfectadas = await _conexion.ExecuteAsync(query, new { Id = id });

            if (filasAfectadas == 0)
            {
                throw new NoDataFoundException("No se encontró el rol para eliminar.");
            }

            _logger.LogInformation("Rol con ID: {IdRol} eliminado exitosamente.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar rol con ID: {IdRol}", id);
            throw new RepositoryException("Ocurrió un error al intentar eliminar el rol.", ex);
        }
    }

    public async Task<Role?> ObtenerPorId(int id)
    {
        string query = "SELECT * FROM Roles WHERE id_rol = @Id";
        return await _conexion.QueryFirstOrDefaultAsync<Role>(query, new { Id = id });
    }

    public async Task<IEnumerable<Role>> ObtenerTodos()
    {
        string query = "SELECT * FROM Roles";
        return await _conexion.QueryAsync<Role>(query);
    }

    public async Task<bool> ExisteNombreRol(string nombreRol)
    {
        string query = "SELECT COUNT(*) FROM Roles WHERE nombre_rol = @NombreRol";
        int count = await _conexion.ExecuteScalarAsync<int>(query, new { NombreRol = nombreRol });
        return count > 0;
    }
}

