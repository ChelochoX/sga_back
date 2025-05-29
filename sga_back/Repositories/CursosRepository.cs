using Dapper;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using System.Data;

namespace sga_back.Repositories;

public class CursosRepository : ICursosRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<CursosRepository> _logger;

    public CursosRepository(ILogger<CursosRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }
    public async Task<int> Insertar(Curso curso)
    {
        try
        {
            _logger.LogInformation("Intentando insertar curso: {Nombre}", curso.Nombre);

            string queryInsertar = @"
            INSERT INTO Cursos (nombre, descripcion, duracion, unidad_duracion, cantidad_cuota, monto_matricula, monto_cuota, tiene_practica, costo_practica, fecha_inicio, fecha_fin)
            VALUES (@Nombre, @Descripcion, @Duracion, @UnidadDuracion, @CantidadCuota, @MontoMatricula, @MontoCuota, @TienePractica, @CostoPractica, @FechaInicio, @FechaFin);
            SELECT CAST(SCOPE_IDENTITY() as int);";

            int id = await _conexion.ExecuteScalarAsync<int>(queryInsertar, curso);
            _logger.LogInformation("Curso insertado con ID: {IdCurso}", id);
            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar curso");
            throw new RepositoryException("Ocurrió un error al intentar insertar el curso.", ex);
        }
    }

    public async Task<int> Actualizar(Curso curso)
    {
        try
        {
            _logger.LogInformation("Intentando actualizar curso con ID: {IdCurso}", curso.IdCurso);

            string query = @"
            UPDATE Cursos
            SET nombre = @Nombre, descripcion = @Descripcion, duracion = @Duracion, unidad_duracion = @UnidadDuracion, 
                cantidad_cuota = @CantidadCuota, monto_matricula = @MontoMatricula, monto_cuota = @MontoCuota, 
                tiene_practica = @TienePractica, costo_practica = @CostoPractica, fecha_inicio = @FechaInicio, fecha_fin = @FechaFin
            WHERE id_curso = @IdCurso";

            int filasAfectadas = await _conexion.ExecuteAsync(query, curso);

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el curso con ID: {IdCurso} para actualizar.", curso.IdCurso);
                throw new NoDataFoundException("No se encontró el curso para actualizar.");
            }

            _logger.LogInformation("Curso con ID: {IdCurso} actualizado exitosamente.", curso.IdCurso);
            return filasAfectadas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar curso con ID: {IdCurso}", curso.IdCurso);
            throw new RepositoryException("Ocurrió un error al intentar actualizar el curso.", ex);
        }
    }

    public async Task<bool> Eliminar(int id)
    {
        try
        {
            _logger.LogInformation("Intentando eliminar curso con ID: {IdCurso}", id);

            string query = "DELETE FROM Cursos WHERE id_curso = @IdCurso";
            int filasAfectadas = await _conexion.ExecuteAsync(query, new { IdCurso = id });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró el curso con ID: {IdCurso} para eliminar.", id);
                return false;
            }

            _logger.LogInformation("Curso con ID: {IdCurso} eliminado exitosamente.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar curso con ID: {IdCurso}", id);
            throw new RepositoryException("Ocurrió un error al intentar eliminar el curso.", ex);
        }
    }

    public async Task<Curso?> ObtenerPorId(int idCurso)
    {
        try
        {
            _logger.LogInformation("Buscando curso con ID: {IdCurso}", idCurso);

            string query = @"
            SELECT 
                id_curso AS IdCurso, 
                nombre AS Nombre, 
                descripcion AS Descripcion, 
                duracion AS Duracion, 
                unidad_duracion AS UnidadDuracion, 
                cantidad_cuota AS CantidadCuota, 
                monto_matricula AS MontoMatricula, 
                monto_cuota AS MontoCuota, 
                tiene_practica AS TienePractica, 
                costo_practica AS CostoPractica, 
                fecha_inicio AS FechaInicio, 
                fecha_fin AS FechaFin
            FROM Cursos 
            WHERE id_curso = @IdCurso";

            Curso? curso = await _conexion.QueryFirstOrDefaultAsync<Curso>(query, new { IdCurso = idCurso });

            if (curso == null)
            {
                _logger.LogWarning("No se encontró el curso con ID: {IdCurso}", idCurso);
            }
            else
            {
                _logger.LogInformation("Curso encontrado: {NombreCurso}", curso.Nombre);
            }

            return curso;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener curso por ID: {IdCurso}", idCurso);
            throw new RepositoryException("Ocurrió un error al obtener el curso.", ex);
        }
    }

    public async Task<IEnumerable<CursoDto>> ObtenerCursosPorFecha(ObtenerCursosRequest request)
    {
        try
        {
            _logger.LogInformation("Obteniendo cursos. Fecha de inicio: {FechaInicio}, Fecha fin: {FechaFin}", request.FechaInicio, request.FechaFin);

            string query = @"
            SELECT 
                id_curso           AS IdCurso,
                nombre             AS Nombre,
                descripcion        AS Descripcion,
                duracion           AS Duracion,
                unidad_duracion    AS UnidadDuracion,
                cantidad_cuota     AS CantidadCuota,
                monto_cuota        AS MontoCuota,
                tiene_practica     AS TienePractica,
                costo_practica     AS CostoPractica,
                fecha_inicio       AS FechaInicio,
                fecha_fin          AS FechaFin,
                monto_matricula    AS MontoMatricula,
                activo             AS Activo
            FROM Cursos
            WHERE 
                (@FechaInicio IS NULL OR fecha_inicio >= @FechaInicio)
                AND (@FechaFin IS NULL OR fecha_inicio <= @FechaFin)
            ORDER BY fecha_inicio DESC
        ";

            var cursos = await _conexion.QueryAsync<CursoDto>(query, new
            {
                FechaInicio = request.FechaInicio,
                FechaFin = request.FechaFin
            });

            _logger.LogInformation("Se obtuvieron {Cantidad} cursos.", cursos.Count());
            return cursos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener los cursos");
            throw new RepositoryException("Ocurrió un error al intentar obtener los cursos.", ex);
        }
    }

    public async Task CambiarEstado(int idCurso, bool activo)
    {
        try
        {
            _logger.LogInformation("Cambiando estado del curso. IdCurso: {IdCurso}, Activo: {Activo}", idCurso, activo);

            string query = @"
            UPDATE Cursos
            SET activo = @Activo
            WHERE id_curso = @IdCurso;
        ";

            int filas = await _conexion.ExecuteAsync(query, new { Activo = activo, IdCurso = idCurso });

            if (filas == 0)
            {
                _logger.LogWarning("No se encontró el curso con IdCurso: {IdCurso} para cambiar estado.", idCurso);
                throw new InvalidOperationException("No se encontró el curso para cambiar estado.");
            }

            _logger.LogInformation("Estado del curso actualizado correctamente. IdCurso: {IdCurso}", idCurso);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del curso con IdCurso: {IdCurso}", idCurso);
            throw new RepositoryException("Ocurrió un error al intentar cambiar el estado del curso.", ex);
        }
    }


}
