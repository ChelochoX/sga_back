using FluentValidation;
using sga_back.Repositories;
using sga_back.Repositories.Interfaces;
using sga_back.Services;
using sga_back.Services.Interfaces;
using System.Reflection;

namespace sga_back.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        _ = services.AddScoped<IPersonasRepository, PersonasRepository>();
        _ = services.AddScoped<ICursosRepository, CursosRepository>();
        _ = services.AddScoped<IUsuariosRepository, UsuariosRepository>();
        _ = services.AddScoped<IRolesRepository, RolesRepository>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        _ = services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        _ = services.AddScoped<IPersonasService, PersonasService>();
        _ = services.AddScoped<ICursosService, CursosService>();
        _ = services.AddScoped<IUsuariosService, UsuariosService>();
        _ = services.AddScoped<IRolesService, RolesService>();
        return services;
    }
}
