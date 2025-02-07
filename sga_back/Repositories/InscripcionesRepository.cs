using Dapper;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using System.Data;

namespace sga_back.Repositories;

public class InscripcionesRepository : IInscripcionesRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<InscripcionesRepository> _logger;

    public InscripcionesRepository(ILogger<InscripcionesRepository> logger, IDbConnection conexion)
    {
        _logger = logger;
        _conexion = conexion;
    }

    public async Task<int> Insertar(Inscripcion inscripcion)
    {
        try
        {
            _logger.LogInformation("Insertando nueva inscripción para persona ID: {IdPersona}, curso ID: {IdCurso}", inscripcion.IdPersona, inscripcion.IdCurso);

            // Verificar si la inscripción ya existe
            string queryVerificar = @"
            SELECT COUNT(*) 
            FROM Inscripciones 
            WHERE id_persona = @IdPersona AND id_curso = @IdCurso AND estado = 'Activa'";

            int existe = await _conexion.ExecuteScalarAsync<int>(queryVerificar, new { inscripcion.IdPersona, inscripcion.IdCurso });

            if (existe > 0)
            {
                _logger.LogWarning("La persona con ID: {IdPersona} ya está inscrita en el curso con ID: {IdCurso}.", inscripcion.IdPersona, inscripcion.IdCurso);
                throw new ReglasdeNegocioException("La persona ya está inscrita en este curso.");
            }

            // Insertar la inscripción
            string queryInsertar = @"
            INSERT INTO Inscripciones (id_persona, id_curso, fecha_inscripcion, estado)
            VALUES (@IdPersona, @IdCurso, @FechaInscripcion, @Estado);
            SELECT CAST(SCOPE_IDENTITY() as int);";

            int id = await _conexion.ExecuteScalarAsync<int>(queryInsertar, inscripcion);
            _logger.LogInformation("Inscripción insertada con ID: {IdInscripcion}", id);

            return id;
        }
        catch (ReglasdeNegocioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar inscripción.");
            throw new RepositoryException("Ocurrió un error al intentar insertar la inscripción.", ex);
        }
    }

    public async Task<int> Actualizar(Inscripcion inscripcion)
    {
        try
        {
            _logger.LogInformation("Actualizando inscripción con ID: {IdInscripcion}", inscripcion.IdInscripcion);

            string query = @"
                    UPDATE Inscripciones
                    SET id_persona = @IdPersona, id_curso = @IdCurso, estado = @Estado
                    WHERE id_inscripcion = @IdInscripcion";

            int filasAfectadas = await _conexion.ExecuteAsync(query, inscripcion);

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró la inscripción con ID: {IdInscripcion} para actualizar.", inscripcion.IdInscripcion);
                throw new NoDataFoundException("No se encontró la inscripción para actualizar.");
            }

            _logger.LogInformation("Inscripción actualizada con éxito.");
            return filasAfectadas;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar inscripción.");
            throw new RepositoryException("Ocurrió un error al intentar actualizar la inscripción.", ex);
        }
    }

    public async Task<bool> Eliminar(int idInscripcion)
    {
        try
        {
            _logger.LogInformation("Eliminando inscripción con ID: {IdInscripcion}", idInscripcion);

            string query = "DELETE FROM Inscripciones WHERE id_inscripcion = @IdInscripcion";
            int filasAfectadas = await _conexion.ExecuteAsync(query, new { IdInscripcion = idInscripcion });

            return filasAfectadas > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar inscripción.");
            throw new RepositoryException("Ocurrió un error al intentar eliminar la inscripción.", ex);
        }
    }

    public async Task<Inscripcion?> ObtenerPorId(int idInscripcion)
    {
        try
        {
            string query = "SELECT * FROM Inscripciones WHERE id_inscripcion = @IdInscripcion";
            return await _conexion.QueryFirstOrDefaultAsync<Inscripcion>(query, new { IdInscripcion = idInscripcion });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inscripción por ID.");
            throw new RepositoryException("Ocurrió un error al intentar obtener la inscripción.", ex);
        }
    }

    public async Task<IEnumerable<Inscripcion>> ObtenerTodas()
    {
        try
        {
            string query = "SELECT * FROM Inscripciones";
            return await _conexion.QueryAsync<Inscripcion>(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las inscripciones.");
            throw new RepositoryException("Ocurrió un error al intentar obtener las inscripciones.", ex);
        }
    }
}
