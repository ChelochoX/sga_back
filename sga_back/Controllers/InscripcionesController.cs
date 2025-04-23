using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sga_back.Middlewares.Filters;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InscripcionesController : ControllerBase
{
    private readonly IInscripcionesService _service;
    private readonly ILogger<InscripcionesController> _logger;

    public InscripcionesController(IInscripcionesService service, ILogger<InscripcionesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    [PermisoRequerido("Crear", "InscripcionesRR")]
    public async Task<IActionResult> Insertar([FromBody] InscripcionRequest request)
    {
        int id = await _service.Insertar(request);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] InscripcionRequest request)
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
        var inscripcion = await _service.ObtenerPorId(id);
        return inscripcion != null ? Ok(inscripcion) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodas()
    {
        var inscripciones = await _service.ObtenerTodas();
        return Ok(inscripciones);
    }

}
