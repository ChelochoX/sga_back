using Microsoft.Extensions.Options;
using sga_back.DTOs;
using sga_back.Exceptions;
using sga_back.Repositories.Interfaces;
using sga_back.Services.Interfaces;

namespace sga_back.Services;

public class FacturaPdfService : IFacturaPdfService
{
    private readonly IFacturaPdfRepository _repository;
    private readonly ILogger<FacturaPdfService> _logger;
    private readonly FacturaPdfSettings _settings;
    private readonly IPagosRepository _repositoryPagos;

    public FacturaPdfService(
        IFacturaPdfRepository repository,
        ILogger<FacturaPdfService> logger,
        IOptions<FacturaPdfSettings> options,
        IPagosRepository repositoryPagos)
    {
        _repository = repository;
        _logger = logger;
        _settings = options.Value;
        _repositoryPagos = repositoryPagos;
    }

    public async Task<byte[]> GenerarPdfPorFactura(int idFactura)
    {
        var factura = await _repository.ObtenerFacturaParaPdf(idFactura);

        if (factura is null)
            throw new RepositoryException($"No se encontró la factura {idFactura}.");

        var config = await _repositoryPagos.ObtenerConfiguracionPorCodigoDocumento("33");

        CompletarConfiguracion(factura, config);

        var document = new FacturaPdfDocument(factura);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerarPdfPorMovimiento(int idMovimiento)
    {
        var idFactura = await _repository.ObtenerIdFacturaPorMovimiento(idMovimiento);

        if (!idFactura.HasValue)
            throw new RepositoryException($"No se encontró factura asociada al movimiento {idMovimiento}.");

        return await GenerarPdfPorFactura(idFactura.Value);
    }

    public async Task<byte[]> GenerarPdfPorAnulacion(int idAnulacion)
    {
        var idFactura = await _repository.ObtenerIdFacturaPorAnulacion(idAnulacion);

        if (!idFactura.HasValue)
            throw new RepositoryException($"No se encontró factura asociada a la anulación {idAnulacion}.");

        return await GenerarPdfPorFactura(idFactura.Value);
    }

    private static void CompletarConfiguracion(FacturaPdfDto factura, DocumentoFiscalConfigDto config)
    {
        factura.RucEmisor = config.RucEmisor;
        factura.Timbrado = config.Timbrado;
        factura.VigenciaDesde = config.VigenciaDesde;
        factura.VigenciaHasta = config.VigenciaHasta;
        factura.RazonSocialEmisor = config.RazonSocialEmisor;
        factura.DireccionEmisor = config.DireccionEmisor;
        factura.TipoTransaccion = "Venta de servicios educativos";
    }
}
