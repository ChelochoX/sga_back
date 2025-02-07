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
            _logger.LogInformation("Intentando eliminar inscripción con ID: {IdInscripcion}", idInscripcion);

            string query = "DELETE FROM Inscripciones WHERE id_inscripcion = @IdInscripcion";
            int filasAfectadas = await _conexion.ExecuteAsync(query, new { IdInscripcion = idInscripcion });

            if (filasAfectadas == 0)
            {
                _logger.LogWarning("No se encontró la inscripción con ID: {IdInscripcion} para eliminar.", idInscripcion);
                return false;
            }

            _logger.LogInformation("Inscripción con ID: {IdInscripcion} eliminada exitosamente.", idInscripcion);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar inscripción con ID: {IdInscripcion}", idInscripcion);
            throw new RepositoryException("Ocurrió un error al intentar eliminar la inscripción.", ex);
        }
    }

    public async Task<Inscripcion?> ObtenerPorId(int idInscripcion)
    {
        try
        {
            _logger.LogInformation("Intentando obtener inscripción con ID: {IdInscripcion}", idInscripcion);

            string query = "SELECT * FROM Inscripciones WHERE id_inscripcion = @IdInscripcion";
            Inscripcion? inscripcion = await _conexion.QueryFirstOrDefaultAsync<Inscripcion>(query, new { IdInscripcion = idInscripcion });

            if (inscripcion == null)
            {
                _logger.LogWarning("No se encontró la inscripción con ID: {IdInscripcion}", idInscripcion);
            }
            else
            {
                _logger.LogInformation("Inscripción obtenida exitosamente.");
            }

            return inscripcion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inscripción con ID: {IdInscripcion}", idInscripcion);
            throw new RepositoryException("Ocurrió un error al intentar obtener la inscripción.", ex);
        }
    }

    public async Task<IEnumerable<Inscripcion>> ObtenerTodas()
    {
        try
        {
            _logger.LogInformation("Obteniendo todas las inscripciones.");

            string query = "SELECT * FROM Inscripciones";
            IEnumerable<Inscripcion> inscripciones = await _conexion.QueryAsync<Inscripcion>(query);

            _logger.LogInformation("Total de inscripciones obtenidas: {TotalInscripciones}", inscripciones.Count());
            return inscripciones;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las inscripciones.");
            throw new RepositoryException("Ocurrió un error al intentar obtener las inscripciones.", ex);
        }
    }
}
