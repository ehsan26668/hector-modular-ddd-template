using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Modules.Projects.Domain;
using Hector.Modules.Projects.Infrastructure;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Hector.Modules.Projects.IntegrationTests.Projects;

public sealed class ProjectsStronglyTypedIdMappingTests
{
    [Fact]
    public async Task Should_PersistAndRehydrateProject_When_AssemblyScanningIsEnabled()
    {
        // Arrange
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var setupContext = CreateContext(connection);
        await setupContext.Database.EnsureCreatedAsync();

        var project = Project.Create("Hector");

        setupContext.Projects.Add(project);
        await setupContext.SaveChangesAsync();

        await using var assertionContext = CreateContext(connection);

        // Act
        var persisted = await assertionContext.Projects.SingleAsync();

        // Assert
        persisted.Id.Should().Be(project.Id);
        persisted.Name.Should().Be(project.Name);
    }

    private static ProjectsDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ProjectsDbContext>()
            .UseSqlite(connection)
            .Options;

        return new ProjectsDbContext(
            options,
            new CompositeStronglyTypedIdAssemblyProvider(
                [
                    new ProjectsStronglyTypedIdAssemblyProvider()
                ]),
            new NoOpSerializer());
    }

    private sealed class NoOpSerializer : IOutboxEventSerializer
    {
        public string GetTypeName(INotification notification)
            => notification.GetType().AssemblyQualifiedName!;

        public string Serialize(INotification notification)
            => "{}";

        public INotification Deserialize(OutboxMessage message)
            => throw new NotSupportedException();
    }
}