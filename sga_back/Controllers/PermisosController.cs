using Microsoft.AspNetCore.Mvc;
using sga_back.DTOs;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermisosController : ControllerBase
{
    private readonly IPermisosService _service;

    public PermisosController(IPermisosService service)
    {
        _service = service;
    }


    [HttpGet("{idRol}/permisos")]
    public async Task<ActionResult<IEnumerable<PermisoDto>>> ObtenerPermisosPorRol(int idRol)
    {
        var permisos = await _service.ObtenerPermisosPorRol(idRol);

        if (permisos == null)
        {
            return NotFound($"No se encontraron permisos para el rol ID: {idRol}");
        }

        return Ok(permisos);
    }

    [HttpGet("entidades-con-recursos")]
    public async Task<IActionResult> ObtenerEntidadesConRecursos()
    {
        var entidadesConRecursos = await _service.ObtenerEntidadesConRecursos();
        return Ok(entidadesConRecursos);

    }

    [HttpPost("asignar-permisos")]
    public async Task<IActionResult> AsignarPermisosARol([FromBody] AsignarPermisosRequest request)
    {
        if (request == null || request.IdRol <= 0 || request.Permisos == null)
        {
            return BadRequest("Datos de entrada inválidos.");
        }

        await _service.AsignarPermisosARol(request.IdRol, request.Permisos);
        return Ok("Permisos asignados correctamente.");

    }
}
