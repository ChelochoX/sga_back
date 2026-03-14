using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FacturasController : ControllerBase
{
    private readonly IFacturaPdfService _service;
    private readonly ILogger<FacturasController> _logger;

    public FacturasController(IFacturaPdfService service, ILogger<FacturasController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("{idFactura:int}/pdf")]
    public async Task<IActionResult> ObtenerPdfPorFactura(int idFactura)
    {
        var pdf = await _service.GenerarPdfPorFactura(idFactura);
        return File(pdf, "application/pdf", $"factura-{idFactura}.pdf");
    }

    [HttpGet("movimiento/{idMovimiento:int}/pdf")]
    public async Task<IActionResult> ObtenerPdfPorMovimiento(int idMovimiento)
    {
        var pdf = await _service.GenerarPdfPorMovimiento(idMovimiento);
        return File(pdf, "application/pdf", $"movimiento-{idMovimiento}-factura.pdf");
    }

    [HttpGet("anulacion/{idAnulacion:int}/pdf")]
    public async Task<IActionResult> ObtenerPdfPorAnulacion(int idAnulacion)
    {
        var pdf = await _service.GenerarPdfPorAnulacion(idAnulacion);
        return File(pdf, "application/pdf", $"anulacion-{idAnulacion}-factura.pdf");
    }
}
