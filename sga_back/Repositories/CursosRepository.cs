using Dapper;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
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

            // Verificar si ya existe un curso con el mismo nombre y rango de fechas
            string queryVerificar = @"
            SELECT COUNT(*) 
            FROM Cursos 
            WHERE nombre = @Nombre AND fecha_inicio = @FechaInicio AND fecha_fin = @FechaFin";

            int existeCurso = await _conexion.ExecuteScalarAsync<int>(queryVerificar, new
            {
                curso.Nombre,
                curso.FechaInicio,
                curso.FechaFin
            });

            if (existeCurso > 0)
            {
                _logger.LogWarning("No se pudo insertar el curso. Ya existe un curso con el nombre {Nombre} entre las fechas {FechaInicio} y {FechaFin}.",
                                    curso.Nombre, curso.FechaInicio, curso.FechaFin);
                throw new ReglasdeNegocioException("Ya existe un curso con el mismo nombre y rango de fechas.");
            }

            // Insertar el curso si no existe duplicado
            string queryInsertar = @"
            INSERT INTO Cursos (nombre, descripcion, duracion, unidad_duracion, costo, fecha_inicio, fecha_fin)
            VALUES (@Nombre, @Descripcion, @Duracion, @UnidadDuracion, @Costo, @FechaInicio, @FechaFin);
            SELECT CAST(SCOPE_IDENTITY() as int);";

            int id = await _conexion.ExecuteScalarAsync<int>(queryInsertar, curso);
            _logger.LogInformation("Curso insertado con ID: {IdCurso}", id);

            return id;
        }
        catch (ReglasdeNegocioException)
        {
            throw;
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
                        costo = @Costo, fecha_inicio = @FechaInicio, fecha_fin = @FechaFin
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
        catch (NoDataFoundException)
        {
            throw;
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
}
