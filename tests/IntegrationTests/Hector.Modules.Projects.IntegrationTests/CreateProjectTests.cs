using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Modules.Projects.Application.Commands;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.IntegrationTests;

public sealed class CreateProjectTests
{
    [Fact]
    public async Task Should_Persist_Project_And_OutboxMessage_When_CreateProjectCommand_IsExecuted()
    {
        // Arrange
        await using var fixture = new ProjectsIntegrationTestFixture();

        using var scope = fixture.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();

        var command = new CreateProjectCommand("New Enterprise Project");

        // Act
        var projectId = await mediator.SendAsync(command, CancellationToken.None);

        // Assert
        var createdProject = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

        createdProject.Should().NotBeNull();
        createdProject!.Name.Should().Be("New Enterprise Project");

        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();

        outboxMessages.Should().HaveCount(1);
        outboxMessages[0].Type.Should().Contain("ProjectCreatedDomainEvent");
    }
}
