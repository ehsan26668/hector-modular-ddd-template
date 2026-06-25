using Hector.BuildingBlocks.Web.Results;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Web;

public static class DependencyInjection
{
    public static IServiceCollection AddHectorWebBuildingBlocks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddGlobalExceptionHandling();

        return services;
    }

    private static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}