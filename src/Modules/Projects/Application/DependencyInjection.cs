using Hector.BuildingBlocks.Application;
using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectsApplication(
        this IServiceCollection services)
    {
        services.AddHectorApplicationBuildingBlocks();

        services.AddMediator(typeof(ProjectsApplicationAssemblyMarker).Assembly);

        return services;
    }
}
