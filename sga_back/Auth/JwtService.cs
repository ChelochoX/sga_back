using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace sga_back.Auth;

public class JwtService
{
    private const string SecretKey = "ClaveSecretaSuperSegura123!";  // 🔒 Usa una clave segura en producción
    private readonly SymmetricSecurityKey _key;

    public JwtService()
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
    }

    public string GenerarToken(int idUsuario, int idRol, string nombreUsuario)
    {
        var claims = new[]
        {
                new Claim("id_usuario", idUsuario.ToString()),
                new Claim("id_rol", idRol.ToString()),
                new Claim("nombre_usuario", nombreUsuario)
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
