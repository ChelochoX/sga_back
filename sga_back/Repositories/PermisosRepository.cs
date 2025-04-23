using Dapper;
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
}
