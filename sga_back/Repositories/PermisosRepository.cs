﻿using Dapper;
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

    public async Task<IEnumerable<EntidadConRecursosDto>> ObtenerEntidadesConRecursos()
    {
        try
        {
            _logger.LogInformation("Actualizando la tabla intermedia Entidad_Recurso con todas las combinaciones posibles.");

            string insertarSql = @"
            INSERT INTO Entidad_Recurso (id_entidad, id_recurso)
            SELECT e.id_entidad, r.id_recurso
            FROM Entidades e
            CROSS JOIN Recursos r
            WHERE NOT EXISTS (
                SELECT 1
                FROM Entidad_Recurso er
                WHERE er.id_entidad = e.id_entidad AND er.id_recurso = r.id_recurso
            );";

            await _conexion.ExecuteAsync(insertarSql);

            _logger.LogInformation("Tabla intermedia actualizada correctamente.");

            string consultaSql = @"
            SELECT 
                e.id_entidad AS IdEntidad,
                e.nombre_entidad AS NombreEntidad,
                r.id_recurso AS IdRecurso,
                r.nombre_recurso AS NombreRecurso
            FROM Entidades e
            INNER JOIN Entidad_Recurso er ON e.id_entidad = er.id_entidad
            INNER JOIN Recursos r ON er.id_recurso = r.id_recurso
            ORDER BY e.nombre_entidad, r.nombre_recurso;";

            var resultado = await _conexion.QueryAsync<EntidadRecursoTempDto>(consultaSql);

            var entidadesDict = new Dictionary<int, EntidadConRecursosDto>();

            foreach (var row in resultado)
            {
                if (!entidadesDict.TryGetValue(row.IdEntidad, out var entidadDto))
                {
                    entidadDto = new EntidadConRecursosDto
                    {
                        IdEntidad = row.IdEntidad,
                        NombreEntidad = row.NombreEntidad,
                        Recursos = new List<RecursoDto>()
                    };
                    entidadesDict.Add(row.IdEntidad, entidadDto);
                }

                entidadDto.Recursos.Add(new RecursoDto
                {
                    IdRecurso = row.IdRecurso,
                    NombreRecurso = row.NombreRecurso
                });
            }

            _logger.LogInformation("Consulta de entidades con recursos ejecutada correctamente.");
            return entidadesDict.Values;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener las entidades con recursos.");
            throw;
        }
    }

    public async Task AsignarPermisosARol(int idRol, List<PermisoDto> permisos)
    {
        try
        {
            // 🔥 Eliminar permisos actuales de ese rol
            string eliminarSql = "DELETE FROM Permisos WHERE id_rol = @idRol";
            await _conexion.ExecuteAsync(eliminarSql, new { idRol });

            // 💾 Insertar nuevos permisos para el rol
            string insertarSql = @"
            INSERT INTO Permisos (id_rol, id_recurso, id_entidad)
            VALUES (@idRol, @idRecurso, @idEntidad)";

            var parametros = permisos.Select(p => new
            {
                idRol,
                idRecurso = p.IdRecurso,
                idEntidad = p.IdEntidad
            });

            await _conexion.ExecuteAsync(insertarSql, parametros);

            _logger.LogInformation("Permisos asignados correctamente al rol {IdRol}", idRol);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar permisos al rol {IdRol}", idRol);
            throw new RepositoryException($"No se pudo asignar permisos al rol {idRol}.", ex);
        }
    }

}
