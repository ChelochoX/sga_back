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

        if ((string.IsNullOrEmpty(usuario.Estado) || usuario.Estado == "Inactivo") && usuario.RequiereCambioContrasena)
            return BadRequest(new { message = "Usuario inactivo. Contacta al administrador." });

        if (string.IsNullOrEmpty(usuario.Estado) && !usuario.RequiereCambioContrasena)
            return BadRequest(new { message = "Credenciales incorrectas." });

        // Si el usuario está inactivo pero requiere cambio de contraseña (primer login)
        if (usuario.Estado == "Activo" && usuario.RequiereCambioContrasena)
        {
            return Ok(new
            {
                RequiereCambioContrasena = true,
                IdUsuario = usuario.IdUsuario,
                NombreUsuario = usuario.NombreUsuario,
                Estado = usuario.Estado,
                Mensaje = "Es necesario cambiar la contraseña antes de continuar."
            });
        }

        // Si el usuario está activo, generamos token
        if (usuario.Estado == "Activo")
        {
            var token = _jwtService.GenerarToken(
                usuario.IdUsuario,
                usuario.IdRol,
                usuario.NombreUsuario
            );

            return Ok(new
            {
                parTokens = new { bearerToken = token }
            });
        }
        // Usuario inactivo sin posibilidad de cambiar contraseña
        return Unauthorized("Usuario inactivo. Contacta al administrador.");
    }

    [HttpPost("cambiar-contrasena")]
    public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaRequest request)
    {
        if (request.NuevaContrasena != request.ConfirmarContrasena)
            return BadRequest("Las contraseñas no coinciden.");

        //obtenemos el usuario id
        var usuario = await _service.ValidarCredenciales(request.Usuario, "");

        request.IdUsuario = usuario.IdUsuario;

        if (!request.IdUsuario.HasValue)
            return BadRequest("No se pudo obtener el ID del usuario.");

        var resultado = await _service.CambiarContrasena(request.IdUsuario.Value, request.NuevaContrasena);

        if (!resultado)
            return BadRequest("No se pudo cambiar la contraseña. Verifica tu información.");

        return Ok("Contraseña cambiada exitosamente. Ahora puedes acceder al sistema.");
    }
}
