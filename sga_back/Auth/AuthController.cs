using Microsoft.AspNetCore.Mvc;
using sga_back.Request;
using sga_back.Services.Interfaces;

namespace sga_back.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUsuariosService _service;
    private readonly JwtService _jwtService;

    public AuthController(IUsuariosService usuarioService, JwtService jwtService)
    {
        _service = usuarioService;
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LogeoRequest request)
    {
        var usuario = await _service.ValidarCredenciales(request.Usuario, request.Contrasena);

        if (usuario == null)
            return Unauthorized("Credenciales incorrectas.");

        var token = _jwtService.GenerarToken(usuario.IdUsuario, usuario.IdRol, usuario.NombreUsuario);

        return Ok(new { Token = token });
    }
}
