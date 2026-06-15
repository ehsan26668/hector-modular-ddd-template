using Hector.BuildingBlocks.Application;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Modules.Projects.Application;
using Hector.Modules.Projects.Contracts;
using Hector.Modules.Projects.Contracts.Events;
using Hector.Modules.Projects.Domain;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hector.Modules.Projects.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectsModule(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddHectorApplicationBuildingBlocks();

        services.AddStronglyTypedIdInfrastructure(
            typeof(ProjectsStronglyTypedIdAssemblyProvider).Assembly);

        services.AddProjectsApplication();
        services.AddProjectsInfrastructure(configureDbContext);

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IIntegrationEventBus, OutboxIntegrationEventBus>();

        return services;
    }

    public static IServiceCollection AddProjectsInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureDbContext);

        services.AddHectorPersistenceBuildingBlocks();

        services.AddOutboxEventContracts(
            typeof(ProjectsContractsAssemblyMarker).Assembly);

        services.AddDbContext<ProjectsDbContext>(configureDbContext);

        services.AddScoped<DbContext>(sp =>
            sp.GetRequiredService<ProjectsDbContext>());

        services.AddScoped<IProjectRepository, ProjectRepository>();

        return services;
    }
}
