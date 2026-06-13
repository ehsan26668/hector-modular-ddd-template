using Hector.BuildingBlocks.Persistence;
using Hector.Modules.Projects.Domain;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectsInfrastructure(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        services.AddHectorPersistenceBuildingBlocks();

        services.AddDbContext<ProjectsDbContext>(configureDbContext);

        services.AddScoped<DbContext>(sp =>
            sp.GetRequiredService<ProjectsDbContext>());

        services.AddScoped<IProjectRepository, ProjectRepository>();

        return services;
    }
}