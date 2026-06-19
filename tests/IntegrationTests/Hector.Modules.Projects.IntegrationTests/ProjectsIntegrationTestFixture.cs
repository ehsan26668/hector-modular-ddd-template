using Hector.Modules.Projects.Infrastructure.Persistence;
using Hector.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;

namespace Hector.Modules.Projects.IntegrationTests;

public sealed class ProjectsIntegrationTestFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly TestApplicationFactory _factory;

    public ProjectsIntegrationTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _factory = new TestApplicationFactory(services =>
        {
            services.RemoveAll<DbContextOptions<ProjectsDbContext>>();

            services.AddSingleton<DbConnection>(_connection);

            services.AddDbContext<ProjectsDbContext>((serviceProvider, options) =>
            {
                var connection = serviceProvider.GetRequiredService<DbConnection>();

                options.UseSqlite(connection);
            });
        });

        using var scope = _factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

        context.Database.EnsureCreated();
    }

    public IServiceScope CreateScope()
    {
        return _factory.Services.CreateScope();
    }

    public void Dispose()
    {
        _factory.Dispose();
        _connection.Dispose();
    }
}
