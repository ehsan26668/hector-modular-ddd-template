using Hector.BuildingBlocks.Application;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Modules.Projects.Application;
using Hector.Modules.Projects.Infrastructure;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.IntegrationTests;

public sealed class ProjectsIntegrationTestFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public IServiceProvider ServiceProvider { get; }

    public ProjectsIntegrationTestFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        services.AddStronglyTypedIdInfrastructure(
            typeof(ProjectsStronglyTypedIdAssemblyProvider).Assembly);

        services.AddHectorApplicationBuildingBlocks();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddSingleton<IOutboxEventSerializer, SystemTextJsonOutboxEventSerializer>();

        services.AddProjectsApplication();

        services.AddProjectsInfrastructure(options =>
        {
            options.UseSqlite(_connection);
        });

        ServiceProvider = services.BuildServiceProvider();

        using var scope = ServiceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

        context.Database.EnsureCreated();
    }


    public IServiceScope CreateScope()
    {
        return ServiceProvider.CreateScope();
    }

    public async ValueTask DisposeAsync()
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        await _connection.DisposeAsync();
    }
}
