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

        var correlationId = correlation?.CorrelationId ??
            (integrationEvent.CorrelationId != Guid.Empty
                ? integrationEvent.CorrelationId
                : integrationEvent.MessageId);

        var causationId = correlation?.CausationId ?? integrationEvent.CausationId;
        var traceId = correlation?.TraceId ?? integrationEvent.TraceId;

        return new OutboxMessage
        {
            Id = integrationEvent.MessageId,
            Type = metadata.Name,
            Version = metadata.Version,
            Content = serializer.Serialize(integrationEvent),
            OccurredOn = DateTime.UtcNow,
            CorrelationId = correlationId,
            CausationId = causationId,
            TraceId = traceId,
            Producer = integrationEvent.GetType().Assembly.GetName().Name ?? "unknown"
        };
    }

}