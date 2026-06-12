using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Persistence;

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
                !t.IsAbstract &&
                !t.IsInterface &&
                t != typeof(CompositeStronglyTypedIdAssemblyProvider))
            .Distinct()
            .ToArray();

        foreach (var providerType in providerTypes)
        {
            services.AddSingleton(providerType);
        }

        services.AddSingleton<IStronglyTypedIdAssemblyProvider>(sp =>
        {
            var providers = providerTypes
                .Select(providerType => (IStronglyTypedIdAssemblyProvider)sp.GetRequiredService(providerType))
                .ToArray();

            return new CompositeStronglyTypedIdAssemblyProvider(providers);
        });

        return services;
    }
}
