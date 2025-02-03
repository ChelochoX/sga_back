using sga_back.Data;
using System.Data;

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
        _ = services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    }
}
