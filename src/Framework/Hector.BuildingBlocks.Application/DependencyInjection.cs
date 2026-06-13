using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHectorApplicationBuildingBlocks(
        this IServiceCollection services)
    {
        services.AddScoped<IMediator, Mediator>();

        services.AddScoped(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        return services;
    }
}