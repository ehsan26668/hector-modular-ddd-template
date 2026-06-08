namespace Hector.BuildingBlocks.Persistence.Outbox;

public interface IOutboxPublisher
{
    Task PublishAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default);
}
