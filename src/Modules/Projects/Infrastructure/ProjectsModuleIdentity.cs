using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.Infrastructure;

public sealed class ProjectsModuleIdentity : IModuleIdentity
{
    public string Name => "Projects";

    public void Register(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddProjectsInfrastructure(configuration);
    }
}
