using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Contracts.Events;
using Hector.Modules.Projects.Domain;

namespace Hector.Modules.Projects.Application.EventHandlers;

public sealed class ProjectCreatedDomainEventHandler(
    IIntegrationEventBus integrationEventBus)
    : INotificationHandler<ProjectCreatedDomainEvent>
{
    public async Task HandleAsync(
        ProjectCreatedDomainEvent notification,
        CancellationToken cancellationToken = default)
    {
        var integrationEvent = new ProjectCreatedIntegrationEvent(
            Guid.NewGuid(),
            notification.ProjectId.Value,
            notification.Name);

        await integrationEventBus.PublishAsync(
            integrationEvent,
            cancellationToken);
    }
}