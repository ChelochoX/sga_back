using Dapper;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using System.Data;

namespace sga_back.Repositories;

public class DocumentosFiscalesConfigRepository : IDocumentosFiscalesConfigRepository
{
    private readonly IDbConnection _conexion;
    private readonly ILogger<DocumentosFiscalesConfigRepository> _logger;

    public DocumentosFiscalesConfigRepository(
        IDbConnection conexion,
        ILogger<DocumentosFiscalesConfigRepository> logger)
    {
        _conexion = conexion;
        _logger = logger;
    }

    public async Task<int> Insertar(DocumentoFiscalConfigRequest request)
    {
        try
        {
            const string sql = @"
                INSERT INTO dbo.DocumentosFiscalesConfig
                (
                    TipoDocumentoId,
                    Sucursal,
                    PuntoExpedicion,
                    Timbrado,
                    NumeroActual,
                    NumeroInicio,
                    NumeroFin,
                    VigenciaDesde,
                    VigenciaHasta,
                    RucEmisor,
                    RazonSocialEmisor,
                    DireccionEmisor,
                    Activo,
                    ConceptoDocumento
                )
                VALUES
                (
                    @TipoDocumentoId,
                    @Sucursal,
                    @PuntoExpedicion,
                    @Timbrado,
                    @NumeroActual,
                    @NumeroInicio,
                    @NumeroFin,
                    @VigenciaDesde,
                    @VigenciaHasta,
                    @RucEmisor,
                    @RazonSocialEmisor,
                    @DireccionEmisor,
                    @Activo,
                    @ConceptoDocumento
                );

                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int id = await _conexion.ExecuteScalarAsync<int>(sql, request);

            _logger.LogInformation("Configuración fiscal insertada con ID: {Id}", id);

            return id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar configuración fiscal.");
            throw new RepositoryException("Ocurrió un error al insertar la configuración fiscal.", ex);
        }
    }

    public async Task<bool> Actualizar(int id, DocumentoFiscalConfigRequest request)
    {
        try
        {
            const string sql = @"
                UPDATE dbo.DocumentosFiscalesConfig
                SET
                    TipoDocumentoId = @TipoDocumentoId,
                    Sucursal = @Sucursal,
                    PuntoExpedicion = @PuntoExpedicion,
                    Timbrado = @Timbrado,
                    NumeroActual = @NumeroActual,
                    NumeroInicio = @NumeroInicio,
                    NumeroFin = @NumeroFin,
                    VigenciaDesde = @VigenciaDesde,
                    VigenciaHasta = @VigenciaHasta,
                    RucEmisor = @RucEmisor,
                    RazonSocialEmisor = @RazonSocialEmisor,
                    DireccionEmisor = @DireccionEmisor,
                    Activo = @Activo,
                    ConceptoDocumento = @ConceptoDocumento
                WHERE Id = @Id;";

            int filas = await _conexion.ExecuteAsync(sql, new
            {
                Id = id,
                request.TipoDocumentoId,
                request.Sucursal,
                request.PuntoExpedicion,
                request.Timbrado,
                request.NumeroActual,
                request.NumeroInicio,
                request.NumeroFin,
                request.VigenciaDesde,
                request.VigenciaHasta,
                request.RucEmisor,
                request.RazonSocialEmisor,
                request.DireccionEmisor,
                request.Activo,
                request.ConceptoDocumento
            });

            if (filas == 0)
                throw new NoDataFoundException("No se encontró la configuración fiscal a actualizar.");

            _logger.LogInformation("Configuración fiscal actualizada con ID: {Id}", id);

            return true;
        }
        catch (NoDataFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar configuración fiscal con ID: {Id}", id);
            throw new RepositoryException("Ocurrió un error al actualizar la configuración fiscal.", ex);
        }
    }

    public async Task<bool> Eliminar(int id)
    {
        try
        {
            const string sql = @"
                DELETE FROM dbo.DocumentosFiscalesConfig
                WHERE Id = @Id;";

            int filas = await _conexion.ExecuteAsync(sql, new { Id = id });

            if (filas == 0)
                throw new NoDataFoundException("No se encontró la configuración fiscal a eliminar.");

            _logger.LogInformation("Configuración fiscal eliminada con ID: {Id}", id);

            return true;
        }
        catch (NoDataFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar configuración fiscal con ID: {Id}", id);
            throw new RepositoryException("Ocurrió un error al eliminar la configuración fiscal.", ex);
        }
    }

    public async Task<DocumentoFiscalConfigDto?> ObtenerPorId(int id)
    {
        try
        {
            const string sql = @"
                SELECT
                    dfc.Id,
                    dfc.TipoDocumentoId,
                    tdf.Nombre AS TipoDocumento,
                    dfc.Sucursal,
                    dfc.PuntoExpedicion,
                    dfc.Timbrado,
                    dfc.NumeroActual,
                    dfc.NumeroInicio,
                    dfc.NumeroFin,
                    dfc.VigenciaDesde,
                    dfc.VigenciaHasta,
                    dfc.RucEmisor,
                    dfc.RazonSocialEmisor,
                    dfc.DireccionEmisor,
                    dfc.Activo,
                    dfc.ConceptoDocumento
                FROM dbo.DocumentosFiscalesConfig dfc
                INNER JOIN dbo.TiposDocumentosFiscales tdf ON tdf.Id = dfc.TipoDocumentoId
                WHERE dfc.Id = @Id;";

            return await _conexion.QueryFirstOrDefaultAsync<DocumentoFiscalConfigDto>(
                sql,
                new { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración fiscal con ID: {Id}", id);
            throw new RepositoryException("Ocurrió un error al obtener la configuración fiscal.", ex);
        }
    }

    public async Task<IEnumerable<DocumentoFiscalConfigDto>> ObtenerTodas(DocumentoFiscalConfigFiltroRequest filtro)
    {
        try
        {
            const string sql = @"
                SELECT
                    dfc.Id,
                    dfc.TipoDocumentoId,
                    tdf.Nombre AS TipoDocumento,
                    dfc.Sucursal,
                    dfc.PuntoExpedicion,
                    dfc.Timbrado,
                    dfc.NumeroActual,
                    dfc.NumeroInicio,
                    dfc.NumeroFin,
                    dfc.VigenciaDesde,
                    dfc.VigenciaHasta,
                    dfc.RucEmisor,
                    dfc.RazonSocialEmisor,
                    dfc.DireccionEmisor,
                    dfc.Activo,
                    dfc.ConceptoDocumento
                FROM dbo.DocumentosFiscalesConfig dfc
                INNER JOIN dbo.TiposDocumentosFiscales tdf ON tdf.Id = dfc.TipoDocumentoId
                WHERE
                    (@TipoDocumentoId IS NULL OR dfc.TipoDocumentoId = @TipoDocumentoId)
                    AND (@Activo IS NULL OR dfc.Activo = @Activo)
                    AND (@ConceptoDocumento IS NULL OR dfc.ConceptoDocumento LIKE '%' + @ConceptoDocumento + '%')
                ORDER BY dfc.Id DESC;";

            return await _conexion.QueryAsync<DocumentoFiscalConfigDto>(sql, new
            {
                filtro.TipoDocumentoId,
                filtro.Activo,
                ConceptoDocumento = string.IsNullOrWhiteSpace(filtro.ConceptoDocumento)
                    ? null
                    : filtro.ConceptoDocumento.Trim()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener listado de configuraciones fiscales.");
            throw new RepositoryException("Ocurrió un error al obtener el listado de configuraciones fiscales.", ex);
        }
    }

    public async Task<IEnumerable<TipoDocumentoFiscalDto>> ObtenerTiposDocumento()
    {
        try
        {
            const string sql = @"
                SELECT
                    Id,
                    CodigoDocumento,
                    Nombre
                FROM dbo.TiposDocumentosFiscales
                WHERE Activo = 1
                ORDER BY Nombre;";

            return await _conexion.QueryAsync<TipoDocumentoFiscalDto>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de documentos fiscales.");
            throw new RepositoryException("Ocurrió un error al obtener los tipos de documentos fiscales.", ex);
        }
    }

    public async Task<bool> ExisteDuplicadoActivo(
        int tipoDocumentoId,
        string sucursal,
        string puntoExpedicion,
        string conceptoDocumento,
        int? idExcluir = null)
    {
        try
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.DocumentosFiscalesConfig
                WHERE TipoDocumentoId = @TipoDocumentoId
                  AND Sucursal = @Sucursal
                  AND PuntoExpedicion = @PuntoExpedicion
                  AND ConceptoDocumento = @ConceptoDocumento
                  AND Activo = 1
                  AND (@IdExcluir IS NULL OR Id <> @IdExcluir);";

            int cantidad = await _conexion.ExecuteScalarAsync<int>(sql, new
            {
                TipoDocumentoId = tipoDocumentoId,
                Sucursal = sucursal,
                PuntoExpedicion = puntoExpedicion,
                ConceptoDocumento = conceptoDocumento,
                IdExcluir = idExcluir
            });

            return cantidad > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar duplicidad de configuración fiscal.");
            throw new RepositoryException("Ocurrió un error al verificar duplicidad de configuración fiscal.", ex);
        }
    }
}
