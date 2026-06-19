using Hector.BuildingBlocks.Application.Messaging.Inbox;
using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Modules.Projects.Contracts;
using Hector.Modules.Projects.Domain;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hector.Modules.Projects.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? optionsOverride = null)
    {
        services.TryAddScoped<IStronglyTypedIdAssemblyProvider, ProjectsStronglyTypedIdAssemblyProvider>();

        // ✅ Correct contract registration (ADR‑0040)
        services.AddOutboxEventContracts(
            typeof(ProjectsContractsAssemblyMarker).Assembly);

        services.TryAddScoped<IInboxConsumerNameProvider>(_ =>
            new StaticInboxConsumerNameProvider("ProjectsModuleConsumer"));

        services.AddDbContext<ProjectsDbContext>(options =>
        {
            if (optionsOverride is not null)
            {
                optionsOverride(options);
                return;
            }

#if (useSqlite)
            var connectionString = configuration.GetConnectionString("Sqlite");
            options.UseSqlite(connectionString);
#elif (usePostgres)
            var connectionString = configuration.GetConnectionString("Postgres");
            options.UseNpgsql(connectionString);
#elif (useSqlServer)
            var connectionString = configuration.GetConnectionString("SqlServer");
            options.UseSqlServer(connectionString);
#endif
        });

        services.AddScoped<HectorDbContext>(sp =>
            sp.GetRequiredService<ProjectsDbContext>());

        services.AddScoped<DbContext>(sp =>
            sp.GetRequiredService<ProjectsDbContext>());

        services.AddScoped<IProjectRepository, ProjectRepository>();

        return services;
    }
}
