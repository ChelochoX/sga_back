using sga_back.Repositories;
using sga_back.Repositories.Interfaces;

namespace sga_back.Configurations;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        _ = services.AddSingleton<IPersonasRepository, PersonasRepository>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        //services.AddSingleton<IMotoService, MotoService>();
        return services;
    }
}
