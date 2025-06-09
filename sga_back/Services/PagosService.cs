using AutoMapper;
using sga_back.Common;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Models;
using sga_back.Repositories.Interfaces;
using sga_back.Request;
using sga_back.Response;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class PagosService : IPagosService
{
    private readonly IPagosRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<PagosService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PagosService(ILogger<PagosService> logger, IPagosRepository repository, IMapper mapper, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _serviceProvider = serviceProvider;
    }

    public async Task<int> InsertarPagoConDetalles(PagoRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        _logger.LogInformation("Insertando nuevo pago para inscripción ID: {IdInscripcion}", request.IdInscripcion);

        PagoEncabezado pago = _mapper.Map<PagoEncabezado>(request);
        List<PagoDetalle> detalles = _mapper.Map<List<PagoDetalle>>(request.Detalles);

        int idPago = await _repository.InsertarPagoConDetalles(pago, detalles);

        _logger.LogInformation("Pago registrado con éxito. ID: {IdPago}", idPago);
        return idPago;
    }

    public async Task<bool> ActualizarPagoConDetalles(int idPago, PagoRequest request)
    {
        await ValidationHelper.ValidarAsync(request, _serviceProvider);

        _logger.LogInformation("Actualizando pago con ID: {IdPago}", idPago);

        PagoEncabezado pago = _mapper.Map<PagoEncabezado>(request);
        pago.IdPago = idPago;

        List<PagoDetalle> detalles = _mapper.Map<List<PagoDetalle>>(request.Detalles);

        bool resultado = await _repository.ActualizarPagoConDetalles(pago, detalles);

        _logger.LogInformation("Pago actualizado con éxito. ID: {IdPago}", idPago);
        return resultado;
    }

    public async Task<bool> EliminarPago(int idPago)
    {
        _logger.LogInformation("Intentando eliminar pago con ID: {IdPago}", idPago);

        bool eliminado = await _repository.EliminarPago(idPago);
        if (!eliminado)
        {
            _logger.LogWarning("No se encontró el pago con ID: {IdPago} para eliminar.", idPago);
            throw new NoDataFoundException("No se encontró el pago para eliminar.");
        }

        _logger.LogInformation("Pago eliminado con éxito. ID: {IdPago}", idPago);
        return eliminado;
    }

    public async Task<PagoResponse?> ObtenerPagoPorId(int idPago)
    {
        _logger.LogInformation("Obteniendo información del pago con ID: {IdPago}", idPago);

        PagoEncabezado? pago = await _repository.ObtenerPagoPorId(idPago);
        if (pago == null)
        {
            _logger.LogWarning("No se encontró el pago con ID: {IdPago}", idPago);
            throw new NoDataFoundException("No se encontró el pago.");
        }

        IEnumerable<PagoDetalle> detalles = await _repository.ObtenerDetallesPorPago(idPago);

        PagoResponse response = _mapper.Map<PagoResponse>(pago);
        response.Detalles = _mapper.Map<List<PagoDetalleResponse>>(detalles);

        _logger.LogInformation("Pago obtenido con éxito. ID: {IdPago}", idPago);
        return response;
    }

    public async Task<(IEnumerable<PagoCabeceraDto> items, int total)> ObtenerPagosPendientes(PagoFiltroRequest filtro)
    {
        return await _repository.ObtenerPagosPendientes(filtro);
    }

    public async Task<(IEnumerable<PagoCabeceraDto> items, int total)> ObtenerPagosRealizados(PagoFiltroRequest filtro)
    {
        return await _repository.ObtenerPagosRealizados(filtro);
    }

    public async Task RegistrarFacturaContado(FacturaContadoRequest request)
    {
        await _repository.RegistrarFacturaContado(request);
    }

    public async Task<DocumentoFiscalConfigDto> ObtenerConfiguracionPorCodigoDocumento(string codigoDocumento)
    {
        return await _repository.ObtenerConfiguracionPorCodigoDocumento(codigoDocumento);
    }
}
