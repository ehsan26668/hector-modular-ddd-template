using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Modules.Projects.Contracts;
using Hector.Modules.Projects.Domain;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? optionsOverride = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddHectorPersistenceBuildingBlocks();

        services.AddSingleton<IModuleIdentity, ProjectsModuleIdentity>();

        services.AddOutboxEventContracts(
            typeof(ProjectsContractsAssemblyMarker).Assembly);

        services.AddDbContext<ProjectsDbContext>(options =>
        {
            if (optionsOverride is not null)
            {
                optionsOverride(options);
                return;
            }

            options.UseSqlServer(
                configuration.GetConnectionString("Projects"));
        });

        services.AddScoped<DbContext>(sp =>
            sp.GetRequiredService<ProjectsDbContext>());

        services.AddScoped<IProjectRepository, ProjectRepository>();

        return services;
    }
}
