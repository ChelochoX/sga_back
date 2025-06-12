using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CajaController : ControllerBase
{
    private readonly ICajaService _service;
    private readonly ILogger<CajaController> _logger;

    public CajaController(ICajaService service, ILogger<CajaController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("anulaciones")]
    public async Task<IActionResult> GetAnulaciones()
    {
        var result = await _service.ObtenerAnulaciones();
        return Ok(result);

    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovimientos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var result = await _service.ObtenerMovimientos(desde, hasta);
        return Ok(result);

    }
}
