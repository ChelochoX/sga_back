using Microsoft.AspNetCore.Mvc;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _service;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(ILogger<UsuariosController> logger, IUsuariosService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Insertar([FromBody] UsuarioRequest request)
    {
        int id = await _service.Insertar(request);
        return Ok(id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] UsuarioUpdateRequest request)
    {
        await _service.ActualizarUsuario(request, id);
        return NoContent();
    }


}
