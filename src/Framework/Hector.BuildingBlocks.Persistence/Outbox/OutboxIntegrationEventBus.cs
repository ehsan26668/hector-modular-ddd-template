using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxIntegrationEventBus(
    DbContext context,
    IOutboxMessageFactory messageFactory)
    : IIntegrationEventBus
{
    public Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var message = messageFactory.Create(integrationEvent);

        context.Set<OutboxMessage>().Add(message);

        return Task.CompletedTask;
    }
}