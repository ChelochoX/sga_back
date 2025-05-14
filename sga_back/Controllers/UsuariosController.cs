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

    [HttpPut("activarusuario/{id}")]
    public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] UsuarioUpdateRequest request)
    {
        await _service.ActualizarUsuario(request, id);
        return NoContent();
    }

    [HttpGet("obtener-usuarios")]
    public async Task<IActionResult> ObtenerUsuarios([FromQuery] string? filtro, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var (usuarios, total) = await _service.ObtenerUsuarios(filtro, pageNumber, pageSize);

        return Ok(new
        {
            usuarios,
            total
        });
    }

    [HttpPut("editar-usuario")]
    public async Task<IActionResult> EditarUsuario([FromBody] UsuarioNameUpdateRequest request)
    {
        await _service.Actualizar(request);
        return NoContent();
    }

    [HttpPut("cambiar-estado/{id}")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        bool actualizado = await _service.CambiarEstadoUsuario(id);
        if (!actualizado)
        {
            return NotFound(new { mensaje = "Usuario no encontrado." });
        }

        return NoContent();
    }
}
