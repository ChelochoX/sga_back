using Microsoft.AspNetCore.Mvc;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PersonasController : ControllerBase
{
    private readonly IPersonasService _service;
    private readonly ILogger<PersonasController> _logger;

    public PersonasController(ILogger<PersonasController> logger, IPersonasService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] PersonaRequest request)
    {
        int id = await _service.Insertar(request);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] PersonaRequest request)
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

    [HttpGet]
    public async Task<IActionResult> ObtenerPersonas()
    {
        var personas = await _service.ObtenerPersonas();
        return Ok(personas);
    }

}
