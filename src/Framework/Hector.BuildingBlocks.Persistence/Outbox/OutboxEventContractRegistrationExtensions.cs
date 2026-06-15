using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public static class OutboxEventContractRegistrationExtensions
{
    public static IServiceCollection AddOutboxEventContracts(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);

        services.Configure<OutboxEventContractOptions>(options =>
        {
            foreach (Assembly assembly in assemblies)
                options.AddAssembly(assembly);
        });

        return services;
    }
}