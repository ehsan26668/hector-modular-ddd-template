using Microsoft.Extensions.DependencyInjection;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Hector.Modules.Projects.Application.Commands;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence;
using System.Reflection;

namespace Hector.Modules.Projects.IntegrationTests.Projects;

public class CreateProjectTests
{
    [Fact]
    public async Task Should_Persist_Project_When_CreateProjectCommand_IsExecuted()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddSingleton<IDomainEventDispatcher, FakeDomainEventDispatcher>();
        services.AddSingleton<IStronglyTypedIdAssemblyProvider, TestStronglyTypedIdAssemblyProvider>();

        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        services.AddDbContext<ProjectsDbContext>(options => options.UseSqlite(connection));

        var mediatorType = typeof(IMediator).Assembly
            .GetType("Hector.BuildingBlocks.Application.Messaging.Mediator")
            ?? throw new InvalidOperationException("Mediator type not found.");

        services.AddScoped(typeof(IMediator), sp =>
            (IMediator)ActivatorUtilities.CreateInstance(sp, mediatorType));

        services.AddScoped<IRequestHandler<CreateProjectCommand, ProjectId>, CreateProjectCommandHandler>();
        services.AddScoped<IProjectRepository, ProjectRepository>();

        var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

        await context.Database.EnsureCreatedAsync();

        var command = new CreateProjectCommand("New Enterprise Project");

        // Act
        var projectId = await mediator.SendAsync(command, CancellationToken.None);

        await context.SaveChangesAsync();

        var createdProject = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

        // Assert
        createdProject.Should().NotBeNull();
        createdProject!.Name.Should().Be("New Enterprise Project");
    }

    public class FakeDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    public class TestStronglyTypedIdAssemblyProvider : IStronglyTypedIdAssemblyProvider
    {
        public IReadOnlyCollection<Assembly> GetAssemblies()
        => new[] { typeof(ProjectId).Assembly };
    }
}
