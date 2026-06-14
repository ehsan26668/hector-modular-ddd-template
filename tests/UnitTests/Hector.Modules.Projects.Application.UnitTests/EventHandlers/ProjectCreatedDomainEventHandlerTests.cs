using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Application.EventHandlers;
using Hector.Modules.Projects.Contracts.Events;
using Hector.Modules.Projects.Domain;
using NSubstitute;

namespace Hector.Modules.Projects.Application.UnitTests.EventHandlers;

public sealed class ProjectCreatedDomainEventHandlerTests
{
    [Fact]
    public async Task Should_PublishIntegrationEvent_When_DomainEventIsReceived()
    {
        // Arrange
        var integrationEventBus = Substitute.For<IIntegrationEventBus>();
        var handler = new ProjectCreatedDomainEventHandler(integrationEventBus);

        var projectId = ProjectId.New();
        var projectName = "Test Project";
        var domainEvent = new ProjectCreatedDomainEvent(projectId, projectName);

        // Act
        await handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await integrationEventBus.Received(1).PublishAsync(
            Arg.Is<ProjectCreatedIntegrationEvent>(e =>
                e.ProjectId == projectId.Value &&
                e.Name == projectName),
            Arg.Any<CancellationToken>());
    }
}