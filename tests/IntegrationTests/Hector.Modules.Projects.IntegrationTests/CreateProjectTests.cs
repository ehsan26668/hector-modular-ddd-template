using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Modules.Projects.Application.Commands;
using Hector.Modules.Projects.Contracts.Events;
using Hector.Modules.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.Modules.Projects.IntegrationTests;

public sealed class CreateProjectTests(
    ProjectsIntegrationTestFixture fixture)
    : IClassFixture<ProjectsIntegrationTestFixture>
{
    [Fact]
    public async Task Should_Execute_Full_ProjectCreation_Pipeline_When_CommandIsSent()
    {
        // Arrange
        using var scope = fixture.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var context = scope.ServiceProvider.GetRequiredService<ProjectsDbContext>();
        var serializer = scope.ServiceProvider.GetRequiredService<IOutboxEventSerializer>();

        var projectName = "New Enterprise Project";
        var command = new CreateProjectCommand(projectName);

        // Act
        var projectId = await mediator.SendAsync(command, CancellationToken.None);

        // Assert

        // 1️⃣ Persistence
        var createdProject = await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

        createdProject.Should().NotBeNull();
        createdProject!.Name.Should().Be(projectName);

        // 2️⃣ Outbox Message existence
        var outboxMessage = await context.Set<OutboxMessage>().SingleAsync();

        outboxMessage.Type.Should().Be("projects.project-created");

        // 3️⃣ Integration event payload (Domain → Integration bridge)
        var deserializedEvent = serializer.Deserialize(outboxMessage);

        deserializedEvent.Should().BeOfType<ProjectCreatedIntegrationEvent>();

        var integrationEvent = (ProjectCreatedIntegrationEvent)deserializedEvent;

        integrationEvent.ProjectId.Should().Be(projectId.Value);
        integrationEvent.Name.Should().Be(projectName);
    }
}
