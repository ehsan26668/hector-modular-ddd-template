using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Contracts.Events;
using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Application.EventHandlers;

public sealed class ProjectCreatedDomainEventHandler(
    IIntegrationEventBus integrationEventBus)
    : INotificationHandler<ProjectCreatedDomainEvent>
{
    public async Task HandleAsync(ProjectCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // 1. Map Domain Event to Integration Event
        var integrationEvent = new ProjectCreatedIntegrationEvent(
            Guid.NewGuid(),
            domainEvent.ProjectId.Value,
            domainEvent.Name
        );

        // 2. Publish Integration Event
        await integrationEventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
