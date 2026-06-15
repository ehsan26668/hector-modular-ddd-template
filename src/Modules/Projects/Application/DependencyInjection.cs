using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectsApplication(
        this IServiceCollection services)
    {
        services.AddSingleton<IModuleIdentity, ProjectsModuleIdentity>();

        services.AddMediator(typeof(ProjectsApplicationAssemblyMarker).Assembly);

        return services;
    }
}
