using sga_back.Common;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class DocumentosFiscalesConfigService : IDocumentosFiscalesConfigService
{
    private readonly IDocumentosFiscalesConfigRepository _repository;
    private readonly ILogger<DocumentosFiscalesConfigService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DocumentosFiscalesConfigService(
        IDocumentosFiscalesConfigRepository repository,
        ILogger<DocumentosFiscalesConfigService> logger,
        IServiceProvider serviceProvider)
    {
        _repository = repository;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<int> Insertar(DocumentoFiscalConfigRequest request)
    {
        try
        {
            await ValidationHelper.ValidarAsync(request, _serviceProvider);

            _logger.LogInformation(
                "Insertando configuración fiscal. TipoDocumentoId: {TipoDocumentoId}, Sucursal: {Sucursal}, PuntoExpedicion: {PuntoExpedicion}, Concepto: {ConceptoDocumento}",
                request.TipoDocumentoId,
                request.Sucursal,
                request.PuntoExpedicion,
                request.ConceptoDocumento);

            bool existeDuplicado = await _repository.ExisteDuplicadoActivo(
                request.TipoDocumentoId,
                request.Sucursal,
                request.PuntoExpedicion,
                request.ConceptoDocumento);

            if (request.Activo && existeDuplicado)
                throw new ReglasdeNegocioException(
                    "Ya existe una configuración activa para ese tipo de documento, sucursal, punto de expedición y concepto.");

            return await _repository.Insertar(request);
        }
        catch (ReglasdeNegocioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al insertar configuración fiscal.");
            throw;
        }
    }

    public async Task<bool> Actualizar(int id, DocumentoFiscalConfigRequest request)
    {
        try
        {
            await ValidationHelper.ValidarAsync(request, _serviceProvider);

            _logger.LogInformation(
                "Actualizando configuración fiscal con ID: {Id}",
                id);

            bool existeDuplicado = await _repository.ExisteDuplicadoActivo(
                request.TipoDocumentoId,
                request.Sucursal,
                request.PuntoExpedicion,
                request.ConceptoDocumento,
                id);

            if (request.Activo && existeDuplicado)
                throw new ReglasdeNegocioException(
                    "Ya existe otra configuración activa para ese tipo de documento, sucursal, punto de expedición y concepto.");

            return await _repository.Actualizar(id, request);
        }
        catch (ReglasdeNegocioException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar configuración fiscal con ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> Eliminar(int id)
    {
        try
        {
            _logger.LogInformation("Eliminando configuración fiscal con ID: {Id}", id);
            return await _repository.Eliminar(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar configuración fiscal con ID: {Id}", id);
            throw;
        }
    }

    public async Task<DocumentoFiscalConfigDto?> ObtenerPorId(int id)
    {
        try
        {
            _logger.LogInformation("Obteniendo configuración fiscal por ID: {Id}", id);
            return await _repository.ObtenerPorId(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración fiscal con ID: {Id}", id);
            throw;
        }
    }

    public async Task<IEnumerable<DocumentoFiscalConfigDto>> ObtenerTodas(DocumentoFiscalConfigFiltroRequest filtro)
    {
        try
        {
            _logger.LogInformation("Obteniendo listado de configuraciones fiscales con filtros: {@Filtro}", filtro);
            return await _repository.ObtenerTodas(filtro);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener listado de configuraciones fiscales.");
            throw;
        }
    }

    public async Task<IEnumerable<TipoDocumentoFiscalDto>> ObtenerTiposDocumento()
    {
        try
        {
            _logger.LogInformation("Obteniendo tipos de documentos fiscales.");
            return await _repository.ObtenerTiposDocumento();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tipos de documentos fiscales.");
            throw;
        }
    }
}
