using Dapper;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Repositories.Interfaces;
using System.Data;

namespace sga_back.Repositories;

public class PermisosRepository : IPermisosRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<PermisosRepository> _logger;

    public PermisosRepository(IDbConnection conexion, ILogger<PermisosRepository> logger)
    {
        _conexion = conexion;
        _logger = logger;
    }

    public async Task<bool> TienePermiso(int idUsuario, string entidad, string recurso)
    {
        try
        {
            string query = @"
                SELECT COUNT(1)
                FROM Usuario_Roles ur
                INNER JOIN Permisos p ON ur.id_rol = p.id_rol
                INNER JOIN Entidades e ON p.id_entidad = e.id_entidad
                INNER JOIN Recursos r ON p.id_recurso = r.id_recurso
                WHERE ur.id_usuario = @IdUsuario
                  AND e.nombre_entidad = @Entidad
                  AND r.nombre_recurso = @Recurso";

            var count = await _conexion.ExecuteScalarAsync<int>(query, new
            {
                IdUsuario = idUsuario,
                Entidad = entidad,
                Recurso = recurso
            });

            if (count > 0)
            {
                _logger.LogInformation("Permiso concedido: Usuario {IdUsuario} puede realizar '{Recurso}' en '{Entidad}'", idUsuario, recurso, entidad);
                return true;
            }

            _logger.LogWarning("Permiso denegado: Usuario {IdUsuario} intentó realizar '{Recurso}' en '{Entidad}' sin permisos asignados.", idUsuario, recurso, entidad);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar permisos para el usuario {IdUsuario}, recurso '{Recurso}', entidad '{Entidad}'", idUsuario, recurso, entidad);
            throw new RepositoryException("Ocurrió un error al verificar permisos.", ex);
        }
    }

    public async Task<IEnumerable<RolDto>> ObtenerRoles()
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los roles...");
            string query = "SELECT id_rol AS IdRol, nombre_rol AS NombreRol FROM Roles";
            return await _conexion.QueryAsync<RolDto>(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles.");
            throw new RepositoryException("Error al obtener los roles.", ex);
        }
    }

    public async Task<IEnumerable<RecursoDto>> ObtenerRecursos()
    {
        try
        {
            _logger.LogInformation("Obteniendo todos los recursos...");
            string query = "SELECT id_recurso AS IdRecurso, nombre_recurso AS NombreRecurso FROM Recursos";
            return await _conexion.QueryAsync<RecursoDto>(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recursos.");
            throw new RepositoryException("Error al obtener los recursos.", ex);
        }
    }

    public async Task<IEnumerable<EntidadDto>> ObtenerEntidades()
    {
        try
        {
            _logger.LogInformation("Obteniendo todas las entidades...");
            string query = "SELECT id_entidad AS IdEntidad, nombre_entidad AS NombreEntidad FROM Entidades";
            return await _conexion.QueryAsync<EntidadDto>(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener entidades.");
            throw new RepositoryException("Error al obtener las entidades.", ex);
        }
    }

    public async Task<IEnumerable<PermisoDto>> ObtenerPermisosPorRol(int idRol)
    {
        try
        {
            _logger.LogInformation("Obteniendo permisos para el rol con ID: {IdRol}", idRol);
            string query = @"
                SELECT id_permiso AS IdPermiso, id_rol AS IdRol, id_recurso AS IdRecurso, id_entidad AS IdEntidad 
                FROM Permisos 
                WHERE id_rol = @IdRol";
            return await _conexion.QueryAsync<PermisoDto>(query, new { IdRol = idRol });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener permisos para el rol: {IdRol}", idRol);
            throw new RepositoryException("Error al obtener permisos.", ex);
        }
    }
}
