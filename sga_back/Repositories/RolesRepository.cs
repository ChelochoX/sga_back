using Dapper;
using sga_back.Common;
using sga_back.DTOs;
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

    public async Task<Rol?> ObtenerPorId(int id)
    {
        string query = "SELECT * FROM Roles WHERE id_rol = @Id";
        return await _conexion.QueryFirstOrDefaultAsync<Rol>(query, new { Id = id });
    }

    public async Task<IEnumerable<Rol>> ObtenerTodos()
    {
        string query = @"SELECT 
                        id_rol AS IdRol, 
                        nombre_rol AS NombreRol 
                     FROM Roles";
        return await _conexion.QueryAsync<Rol>(query);
    }

    public async Task<bool> ExisteNombreRol(string nombreRol)
    {
        string query = "SELECT COUNT(*) FROM Roles WHERE nombre_rol = @NombreRol";
        int count = await _conexion.ExecuteScalarAsync<int>(query, new { NombreRol = nombreRol });
        return count > 0;
    }

    public async Task<IEnumerable<RolDetalleDto>> ObtenerDetalleRolesPorNombreUsuario(string nombreUsuario)
    {
        try
        {
            _logger.LogInformation("Buscando roles para el usuario con nombre similar a: {NombreUsuario}", nombreUsuario);

            string sql = @"
            SELECT
                r.id_rol          AS IdRol,
                r.nombre_rol      AS NombreRol,
                e.id_entidad      AS IdEntidad,
                e.nombre_entidad  AS NombreEntidad,
                re.nombre_recurso AS NombreAccion,
                u.nombre_usuario As NombreUsuario
            FROM Usuarios u
            LEFT JOIN Usuario_Roles ur   ON ur.id_usuario = u.id_usuario
            LEFT JOIN Roles r            ON r.id_rol      = ur.id_rol
            LEFT JOIN Permisos p         ON p.id_rol      = r.id_rol
            LEFT JOIN Recursos re        ON re.id_recurso = p.id_recurso
            LEFT JOIN Entidades e        ON e.id_entidad  = p.id_entidad
            WHERE LOWER(u.nombre_usuario) LIKE LOWER(@nombreParam)";

            var nombreParam = $"%{nombreUsuario}%";

            var rows = await _conexion.QueryAsync<RolDetalleTemp>(sql, new { nombreParam });
            var lookup = new Dictionary<int, RolDetalleDto>();

            foreach (var row in rows)
            {
                if (!lookup.TryGetValue(row.IdRol, out var rolDto))
                {
                    rolDto = new RolDetalleDto
                    {
                        IdRol = row.IdRol,
                        NombreRol = row.NombreRol,
                        NombreUsuario = row.NombreUsuario
                    };
                    lookup[row.IdRol] = rolDto;
                }

                var entidad = rolDto.Entidades.FirstOrDefault(e => e.IdEntidad == row.IdEntidad);

                if (entidad == null && row.IdEntidad.HasValue)
                {
                    entidad = new EntidadDetalleDto
                    {
                        IdEntidad = row.IdEntidad.Value,
                        NombreEntidad = row.NombreEntidad
                    };
                    rolDto.Entidades.Add(entidad);
                }

                if (!string.IsNullOrWhiteSpace(row.NombreAccion) &&
                    entidad != null &&
                    !entidad.Acciones.Contains(row.NombreAccion))
                {
                    entidad.Acciones.Add(row.NombreAccion);
                }
            }

            _logger.LogInformation("Se obtuvieron {Count} roles con permisos para el usuario.", lookup.Count);
            return lookup.Values;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el detalle de roles del usuario: {NombreUsuario}", nombreUsuario);
            throw new RepositoryException("Ocurrió un error al intentar obtener los roles del usuario.", ex);
        }
    }

    // clase interna temporal para mapear columnas crudas
    private record RolDetalleTemp
    (
        int IdRol,
        string NombreRol,
        int? IdEntidad,
        string NombreEntidad,
        string NombreAccion,
        string NombreUsuario
    );

    public async Task<IEnumerable<int>> ObtenerIdsRolesPorUsuario(string nombreUsuario)
    {
        try
        {
            string sql = @"
                SELECT r.id_rol
                FROM Usuarios u
                INNER JOIN Usuario_Roles ur ON u.id_usuario = ur.id_usuario
                INNER JOIN Roles r ON ur.id_rol = r.id_rol
                WHERE u.nombre_usuario = @nombreUsuario";

            return await _conexion.QueryAsync<int>(sql, new { nombreUsuario });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener roles del usuario {NombreUsuario}", nombreUsuario);
            throw new RepositoryException($"No se pudieron obtener los roles del usuario {nombreUsuario}", ex);
        }
    }

    public async Task AsignarRolAUsuario(string nombreUsuario, int idRol)
    {
        try
        {
            string sql = @"
                INSERT INTO Usuario_Roles (id_usuario, id_rol)
                SELECT u.id_usuario, @idRol
                FROM Usuarios u
                WHERE u.nombre_usuario = @nombreUsuario
                AND NOT EXISTS (
                    SELECT 1 FROM Usuario_Roles ur
                    WHERE ur.id_usuario = u.id_usuario AND ur.id_rol = @idRol
                )";

            await _conexion.ExecuteAsync(sql, new { nombreUsuario, idRol });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar el rol {IdRol} al usuario {NombreUsuario}", idRol, nombreUsuario);
            throw new RepositoryException($"No se pudo asignar el rol {idRol} al usuario {nombreUsuario}", ex);
        }
    }

    public async Task RemoverRolDeUsuario(string nombreUsuario, int idRol)
    {
        try
        {
            string sql = @"
                DELETE ur
                FROM Usuario_Roles ur
                INNER JOIN Usuarios u ON ur.id_usuario = u.id_usuario
                WHERE u.nombre_usuario = @nombreUsuario AND ur.id_rol = @idRol";

            await _conexion.ExecuteAsync(sql, new { nombreUsuario, idRol });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover el rol {IdRol} del usuario {NombreUsuario}", idRol, nombreUsuario);
            throw new RepositoryException($"No se pudo remover el rol {idRol} del usuario {nombreUsuario}", ex);
        }
    }

    public async Task<string> ObtenerNombreRolPorId(int idRol)
    {
        try
        {
            var sql = "SELECT nombre_rol FROM Roles WHERE id_rol = @idRol";
            return await _conexion.QueryFirstOrDefaultAsync<string>(sql, new { idRol });

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar roles del rol {IdRol}", idRol);
            throw new RepositoryException($"No se pudo eliminar los permisos del rol {idRol}", ex);
        }
    }

    public async Task EliminarPermisosDeRol(int idRol)
    {
        try
        {
            const string sql = @"DELETE FROM Permisos WHERE id_rol = @idRol";
            await _conexion.ExecuteAsync(sql, new { idRol });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar los permisos del rol {IdRol}", idRol);
            throw new RepositoryException($"No se pudo eliminar los permisos del rol {idRol}", ex);
        }
    }

    public async Task InsertarPermisosPredefinidos(int idRol, string nombreRol)
    {
        try
        {
            var permisos = PermisosPredefinidosProvider.ObtenerPorRol(nombreRol);

            foreach (var (entidad, recurso) in permisos)
            {
                var sql = @"
                INSERT INTO Permisos (id_rol, id_entidad, id_recurso)
                SELECT @idRol, e.id_entidad, r.id_recurso
                FROM Entidades e, Recursos r
                WHERE e.nombre_entidad = @entidad AND r.nombre_recurso = @recurso";

                await _conexion.ExecuteAsync(sql, new { idRol, entidad, recurso });
            }

            _logger.LogInformation("Permisos predefinidos insertados para el rol {IdRol} - {NombreRol}", idRol, nombreRol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar permisos predefinidos para el rol {IdRol}", idRol);
            throw new RepositoryException($"No se pudieron insertar permisos predefinidos para el rol {idRol}", ex);
        }
    }

}

