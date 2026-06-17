using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Messaging;

public static class ModuleLoade
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var moduleTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IModuleIdentity).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface);

        foreach (var moduleType in moduleTypes)
        {
            var module = (IModuleIdentity)Activator.CreateInstance(moduleType)!;

            module.Register(services, configuration);
        }

        return services;
    }
}