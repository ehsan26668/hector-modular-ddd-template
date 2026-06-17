using Hector.BuildingBlocks.Application;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence;
using Hector.Modules.Projects.Application;
using Hector.Modules.Projects.Infrastructure;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.IntegrationTests;

public sealed class ProjectsIntegrationTestFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;

    public ProjectsIntegrationTestFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder().Build();

        services.AddHectorApplicationBuildingBlocks();

        services.AddProjectsApplication();

        services.AddProjectsInfrastructure(
            configuration,
            options =>
            {
                options.UseSqlite(_connection);
            });

        services.AddSingleton<IStronglyTypedIdAssemblyProvider, ProjectsStronglyTypedIdAssemblyProvider>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        _serviceProvider = services.BuildServiceProvider();

        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
        context.Database.EnsureCreated();
    }

    public IServiceScope CreateScope() => _serviceProvider.CreateScope();

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
