using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Correlation;
using Hector.Modules.Projects.Application.EventHandlers;
using Hector.Modules.Projects.Contracts.Events;
using Hector.Modules.Projects.Domain;
using NSubstitute;

namespace Hector.Modules.Projects.Application.UnitTests.EventHandlers;

public sealed class ProjectCreatedDomainEventHandlerTests
{
    private readonly IIntegrationEventBus _integrationEventBus = Substitute.For<IIntegrationEventBus>();
    private readonly ICorrelationContextAccessor _correlationContextAccessor = Substitute.For<ICorrelationContextAccessor>();
    private readonly ProjectCreatedDomainEventHandler _handler;

    public ProjectCreatedDomainEventHandlerTests()
    {
        _handler = new ProjectCreatedDomainEventHandler(_integrationEventBus, _correlationContextAccessor);
    }

    [Fact]
    public async Task Should_PublishIntegrationEvent_When_DomainEventIsReceived()
    {
        // Arrange
        var projectId = ProjectId.New();
        const string projectName = "Test Project";
        var domainEvent = new ProjectCreatedDomainEvent(projectId, projectName);

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _integrationEventBus.Received(1).PublishAsync(
            Arg.Is<ProjectCreatedIntegrationEvent>(e =>
                e.MessageId != Guid.Empty &&
                e.ProjectId == projectId.Value &&
                e.Name == projectName),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_PropagateCorrelationMetadata_When_CorrelationContextExists()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var causationId = Guid.NewGuid();
        const string traceId = "trace-123";

        _correlationContextAccessor.Current.Returns(new CorrelationContext(
            correlationId,
            causationId,
            traceId));

        var domainEvent = new ProjectCreatedDomainEvent(ProjectId.New(), "Project with Correlation");

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _integrationEventBus.Received(1).PublishAsync(
            Arg.Is<ProjectCreatedIntegrationEvent>(e =>
                e.CorrelationId == correlationId &&
                e.CausationId == causationId &&
                e.TraceId == traceId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_UseMessageIdAsCorrelationId_When_CorrelationContextDoesNotExist()
    {
        // Arrange
        _correlationContextAccessor.Current.Returns((CorrelationContext?)null);

        var domainEvent = new ProjectCreatedDomainEvent(ProjectId.New(), "Project without Context");

        // Act
        await _handler.HandleAsync(domainEvent, CancellationToken.None);

        // Assert
        await _integrationEventBus.Received(1).PublishAsync(
            Arg.Is<ProjectCreatedIntegrationEvent>(e =>
                e.CorrelationId == e.MessageId &&
                e.CausationId == null),
            Arg.Any<CancellationToken>());
    }
}
