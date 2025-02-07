using Microsoft.AspNetCore.Mvc;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuarioRolesController : ControllerBase
{
    private readonly IUsuarioRolesService _service;
    private readonly ILogger<UsuarioRolesController> _logger;

    public UsuarioRolesController(IUsuarioRolesService service, ILogger<UsuarioRolesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AsignarRol(int idUsuario, int idRol)
    {
        bool resultado = await _service.AsignarRol(idUsuario, idRol);
        return resultado ? Ok("Rol asignado exitosamente.") : BadRequest("Error al asignar el rol.");
    }

    [HttpDelete]
    public async Task<IActionResult> EliminarRol(int idUsuario, int idRol)
    {
        bool resultado = await _service.EliminarRol(idUsuario, idRol);
        return resultado ? Ok("Rol eliminado exitosamente.") : NotFound("No se encontró la relación rol-usuario.");
    }

    [HttpGet("{idUsuario}")]
    public async Task<IActionResult> ObtenerRoles(int idUsuario)
    {
        var roles = await _service.ObtenerRolesPorUsuario(idUsuario);
        return Ok(roles);
    }
}
