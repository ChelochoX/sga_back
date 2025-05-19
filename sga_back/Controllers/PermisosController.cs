using Microsoft.AspNetCore.Mvc;
using sga_back.DTOs;
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

    [HttpGet("roles")]
    public async Task<ActionResult<IEnumerable<RolDto>>> ObtenerRoles()
    {
        var roles = await _service.ObtenerRoles();
        return Ok(roles);
    }

    [HttpGet("recursos")]
    public async Task<ActionResult<IEnumerable<RecursoDto>>> ObtenerRecursos()
    {
        var recursos = await _service.ObtenerRecursos();
        return Ok(recursos);
    }

    [HttpGet("entidades")]
    public async Task<ActionResult<IEnumerable<EntidadDto>>> ObtenerEntidades()
    {
        var entidades = await _service.ObtenerEntidades();
        return Ok(entidades);
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

}
