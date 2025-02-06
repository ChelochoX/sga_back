using Microsoft.AspNetCore.Mvc;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRolesService _service;

    public RolesController(IRolesService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] RoleRequest request)
    {
        int id = await _service.Insertar(request);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] RoleRequest request)
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
        var role = await _service.ObtenerPorId(id);
        return role != null ? Ok(role) : NotFound();
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var roles = await _service.ObtenerTodos();
        return Ok(roles);
    }
}
