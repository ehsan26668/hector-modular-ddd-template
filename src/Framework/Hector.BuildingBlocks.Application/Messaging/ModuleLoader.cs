using System.Reflection;
using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Modules;

public static class ModuleLoader
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var assemblies = DiscoverModuleAssemblies();

        var modules = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IModule).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .Select(t => (IModule)Activator.CreateInstance(t)!);

        foreach (var module in modules)
        {
            module.Register(services, configuration);
        }

        return services;
    }

    private static IEnumerable<Assembly> DiscoverModuleAssemblies()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a =>
                a.GetName().Name!.StartsWith("Hector.Modules."));
    }
}
