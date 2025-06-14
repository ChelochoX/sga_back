using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace sga_back.Auth;

public class JwtService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(IConfiguration configuration)
    {
        var secretKey = configuration["Jwt:Key"];
        _issuer = configuration["Jwt:Issuer"];
        _audience = configuration["Jwt:Audience"];

        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(_issuer) || string.IsNullOrEmpty(_audience))
        {
            throw new ArgumentException("Faltan configuraciones JWT.");
        }

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
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
              issuer: _issuer,
              audience: _audience,
              expires: DateTime.UtcNow.AddHours(24),
              claims: claims,
              signingCredentials: creds
          );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
