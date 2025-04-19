using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace sga_back.Auth;

public class JwtService
{
    private readonly SymmetricSecurityKey _key;

    public JwtService(IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new ArgumentException("La clave JWT no está configurada en el appsettings.json.");
        }

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    public string GenerarToken(int idUsuario, int idRol, string nombreUsuario, bool requiereCambioContrasena)
    {
        var claims = new[]
        {
                new Claim("id_usuario", idUsuario.ToString()),
                new Claim("id_rol", idRol.ToString()),
                new Claim("nombre_usuario", nombreUsuario),
                new Claim("requiere_cambio_contrasena", requiereCambioContrasena.ToString())
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(3),  // ⏳ Duración del token
            claims: claims,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
