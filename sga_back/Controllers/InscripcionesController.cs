using Microsoft.AspNetCore.Mvc;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
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
    //[PermisoRequerido("Crear", "Inscripciones")]
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
    public async Task<IActionResult> ObtenerTodas([FromQuery] InscripcionFiltroRequest filtro)
    {
        var lista = await _service.ObtenerTodas(filtro);
        return Ok(lista);
    }

    [HttpGet("estudiantes")]
    public async Task<IActionResult> ObtenerEstudiantes([FromQuery] string? q)
    {
        var estudiantes = await _service.ObtenerEstudiantes(q);
        return Ok(estudiantes);
    }

    [HttpGet("obtener-cursos")]
    public async Task<IActionResult> ObtenerCursos([FromQuery] string? search)
    {
        var cursos = await _service.ObtenerCursos(search);
        return Ok(cursos);
    }
}
