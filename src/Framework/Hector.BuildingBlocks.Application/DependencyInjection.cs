using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Correlation;
using Hector.BuildingBlocks.Application.Messaging.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hector.BuildingBlocks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHectorApplicationBuildingBlocks(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(
            ServiceDescriptor.Scoped(
                typeof(IPipelineBehavior<,>),
                typeof(ValidationBehavior<,>)));

        services.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();

        return services;
    }
}
