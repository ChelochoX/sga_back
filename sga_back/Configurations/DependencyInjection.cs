﻿using FluentValidation;
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
        _ = services.AddSingleton<IPersonasRepository, PersonasRepository>();
        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        _ = services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        _ = services.AddScoped<IPersonasService, PersonasService>();
        return services;
    }
}
