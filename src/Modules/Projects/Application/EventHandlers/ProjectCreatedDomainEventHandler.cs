using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Correlation;
using Hector.Modules.Projects.Contracts.Events;
using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Application.EventHandlers;

public sealed class ProjectCreatedDomainEventHandler(
    IIntegrationEventBus integrationEventBus,
    ICorrelationContextAccessor correlationContextAccessor)
    : INotificationHandler<ProjectCreatedDomainEvent>
{
    public async Task HandleAsync(ProjectCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var context = correlationContextAccessor.Current;
        var messageId = Guid.NewGuid();

        var integrationEvent = new ProjectCreatedIntegrationEvent(
            messageId,
            context?.CorrelationId ?? messageId,
            context?.CausationId,
            context?.TraceId,
            domainEvent.ProjectId.Value,
            domainEvent.Name);

        await integrationEventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
