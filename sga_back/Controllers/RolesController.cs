using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sga_back.DTOs;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRolesService _service;

    public RolesController(IRolesService service)
    {
        _service = service;
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var rol = await _service.ObtenerPorId(id);
        return rol != null ? Ok(rol) : NotFound();
    }

    [HttpGet("obtener-todos")]
    public async Task<IActionResult> ObtenerTodos()
    {
        var roles = await _service.ObtenerTodos();
        return Ok(roles);
    }


    [HttpGet("usuarios/detalle-roles")]
    public async Task<IActionResult> GetDetalleRoles([FromQuery] string nombreUsuario)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario))
            return BadRequest("El nombre de usuario es obligatorio.");

        var data = await _service.ObtenerDetalleRolesPorNombreUsuario(nombreUsuario);
        return Ok(data);
    }


    [HttpPost("actualizar-roles")]
    public async Task<IActionResult> ActualizarRoles([FromBody] ActualizarRolesRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NombreUsuario))
        {
            return BadRequest("El nombre de usuario es obligatorio.");
        }

        await _service.ActualizarRolesUsuario(request.NombreUsuario, request.IdsRoles);
        return Ok("Roles actualizados correctamente.");
    }

}
