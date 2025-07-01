using Microsoft.AspNetCore.Mvc;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class CajaController : ControllerBase
{
    private readonly ICajaService _service;
    private readonly ILogger<CajaController> _logger;

    public CajaController(ICajaService service, ILogger<CajaController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovimientos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var result = await _service.ObtenerMovimientos(desde, hasta);
        return Ok(result);

    }

    [HttpPost("anular-movimiento")]
    public async Task<IActionResult> AnularMovimiento([FromBody] AnulacionRequest request)
    {
        await _service.AnularMovimientoCaja(request.IdMovimiento, request.Motivo);
        return Ok(new { mensaje = "Movimiento anulado correctamente." });
    }

    [HttpGet("anulaciones")]
    public async Task<IActionResult> GetAnulaciones([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var anulaciones = await _service.ObtenerAnulaciones(desde, hasta);
        return Ok(anulaciones);
    }
}
