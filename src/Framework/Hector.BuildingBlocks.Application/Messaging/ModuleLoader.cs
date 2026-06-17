using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Messaging;

public static class ModuleLoader
{
    public static IServiceCollection AddModules(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var moduleIdentities = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IModuleIdentity).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .ToList();

        foreach (var moduleType in moduleIdentities)
        {
            var module = (IModuleIdentity)Activator.CreateInstance(moduleType)!;

            RegisterModule(services, moduleType.Assembly);
        }

        return services;
    }

    private static void RegisterModule(
        IServiceCollection services,
        Assembly moduleAssembly)
    {
        var dependencyInjectionType = moduleAssembly
            .GetTypes()
            .FirstOrDefault(t =>
                t.Name == "DependencyInjection" &&
                t.IsSealed &&
                t.IsAbstract);

        if (dependencyInjectionType is null)
            return;

        var method = dependencyInjectionType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m =>
                m.Name == "AddApplication" ||
                m.Name == "AddInfrastructure");

        if (method is null)
            return;

        method.Invoke(null, [services]);
    }
}