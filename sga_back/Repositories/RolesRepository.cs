using Dapper;
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

    public async Task<int> Insertar(Rol rol)
    {
        try
        {
            _logger.LogInformation("Intentando insertar rol: {NombreRol}", rol.NombreRol);

            if (await ExisteNombreRol(rol.NombreRol))
            {
                throw new ReglasdeNegocioException("El nombre del rol ya está registrado.");
            }

            string query = "INSERT INTO Roles (nombre_rol) VALUES (@NombreRol); SELECT CAST(SCOPE_IDENTITY() as int);";
            int id = await _conexion.ExecuteScalarAsync<int>(query, rol);

            _logger.LogInformation("Rol insertado con ID: {IdRol}", id);
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar rol.");
            throw new RepositoryException("Ocurrió un error al intentar insertar el rol.", ex);
        }
    }

    public async Task<int> Actualizar(Rol role)
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
                re.nombre_recurso AS NombreAccion
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
                        NombreRol = row.NombreRol
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
        string NombreAccion
    );

}

