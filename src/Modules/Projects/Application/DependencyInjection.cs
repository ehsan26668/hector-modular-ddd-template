using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Application.Commands;
using Hector.Modules.Projects.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectsApplication(this IServiceCollection services)
    {
        services.AddScoped<IRequestHandler<CreateProjectCommand, ProjectId>, CreateProjectCommandHandler>();

        return services;
    }
}
