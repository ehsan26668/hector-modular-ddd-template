using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxPublisher(
    IMediator mediator,
    IOutboxEventSerializer serializer)
    : IOutboxPublisher
{

    public async Task PublishAsync(
        IEnumerable<OutboxMessage> messages,
        CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
        {
            var domainEvent = serializer.Deserialize(message);

            await mediator.PublishAsync(
                domainEvent,
                cancellationToken);
        }
    }
}
