using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Correlation;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class DefaultOutboxMessageFactory(
    IOutboxEventSerializer serializer,
    IOutboxEventTypeResolver typeResolver,
    ICorrelationContextAccessor correlationContextAccessor)
    : IOutboxMessageFactory
{
    public OutboxMessage Create(IIntegrationEvent integrationEvent)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        var metadata = typeResolver.GetMetadata(integrationEvent.GetType());
        var correlation = correlationContextAccessor.Current;

        return new OutboxMessage
        {
            Id = integrationEvent.MessageId,
            Type = metadata.Name,
            Version = metadata.Version,
            Content = serializer.Serialize(integrationEvent),
            OccurredOn = DateTime.UtcNow,
            CorrelationId = correlation?.CorrelationId ?? integrationEvent.MessageId,
            CausationId = correlation?.CausationId,
            TraceId = correlation?.TraceId,
            Producer = integrationEvent.GetType().Assembly.GetName().Name ?? "unknown"
        };
    }
}