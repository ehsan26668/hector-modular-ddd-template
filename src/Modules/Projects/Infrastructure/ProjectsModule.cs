using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.Infrastructure;

public sealed class ProjectsModule : IModule
{
    public void Register(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddProjectsApplication();
        services.AddProjectsInfrastructure(configuration);
    }
}
