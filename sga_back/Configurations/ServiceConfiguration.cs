using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using sga_back.Auth;
using sga_back.Data;
using System.Data;
using System.Reflection;
using System.Text;

namespace sga_back.Configurations;

public static class ServiceConfiguration
{
    public static void AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddSingleton<DbConnections>();

        _ = services.AddTransient<IDbConnection>(sp =>
            {
                var dbConnections = sp.GetRequiredService<DbConnections>();
                return dbConnections.CreateSqlConnection();  // Usar la conexión creada por DbConnections
            });

        _ = services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy =>
                {
                    _ = policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
        });

        // ✅ Registro del JwtService aquí
        services.AddSingleton<JwtService>();

        // ✅ Configuración de Autenticación JWT
        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                var key = configuration["Jwt:Key"];
                var issuer = configuration["Jwt:Issuer"];
                var audience = configuration["Jwt:Audience"];

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,

                    ValidIssuer = issuer,
                    ValidAudience = audience
                };
            });

        // Registro de AutoMapper
        services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        // Registro de validadores con FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
