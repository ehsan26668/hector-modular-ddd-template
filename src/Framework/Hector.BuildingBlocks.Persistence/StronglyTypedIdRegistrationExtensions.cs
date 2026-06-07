using System.Reflection;
using Hector.BuildingBlocks.Persistence;
using Microsoft.Extensions.DependencyInjection;

public static class StronglyTypedIdRegistrationExtensions
{
    public static IServiceCollection AddStronglyTypedIdInfrastructure(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var providerTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IStronglyTypedIdAssemblyProvider).IsAssignableFrom(t) &&
                !t.IsInterface &&
                !t.IsAbstract)
            .ToList();

        foreach (var type in providerTypes)
        {
            services.AddSingleton(typeof(IStronglyTypedIdAssemblyProvider), type);
        }

        services.AddSingleton<IStronglyTypedIdAssemblyProvider,
            CompositeStronglyTypedIdAssemblyProvider>();

        return services;
    }
}
