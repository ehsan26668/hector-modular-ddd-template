using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class DefaultOutboxMessageFactory(
    IOutboxEventSerializer serializer,
    IOutboxEventTypeResolver typeResolver)
    : IOutboxMessageFactory
{
    public OutboxMessage Create(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var metadata = typeResolver.GetMetadata(integrationEvent.GetType());

        return new OutboxMessage
        {
            Id = integrationEvent.MessageId,
            Type = metadata.Name,
            Version = metadata.Version,
            Content = serializer.Serialize(integrationEvent),
            OccurredOn = DateTime.UtcNow
        };
    }
}