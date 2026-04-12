using Microsoft.AspNetCore.Mvc;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentosFiscalesConfigController : ControllerBase
{
    private readonly IDocumentosFiscalesConfigService _service;
    private readonly ILogger<DocumentosFiscalesConfigController> _logger;

    public DocumentosFiscalesConfigController(
        IDocumentosFiscalesConfigService service,
        ILogger<DocumentosFiscalesConfigController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] DocumentoFiscalConfigRequest request)
    {
        int id = await _service.Insertar(request);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] DocumentoFiscalConfigRequest request)
    {
        await _service.Actualizar(id, request);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.Eliminar(id);
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var item = await _service.ObtenerPorId(id);
        return item != null ? Ok(item) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas([FromQuery] DocumentoFiscalConfigFiltroRequest filtro)
    {
        var lista = await _service.ObtenerTodas(filtro);
        return Ok(lista);
    }

    [HttpGet("tipos-documento")]
    public async Task<IActionResult> ObtenerTiposDocumento()
    {
        var lista = await _service.ObtenerTiposDocumento();
        return Ok(lista);
    }
}
