using System.Diagnostics;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Correlation;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxPublisher(
    IMediator mediator,
    IOutboxEventSerializer serializer,
    ICorrelationContextAccessor correlationContextAccessor)
    : IOutboxPublisher
{
    // ✅ Constructor مخصوص تست (Backward Compatible)
    public OutboxPublisher(
        IMediator mediator,
        IOutboxEventSerializer serializer)
        : this(
            mediator,
            serializer,
            new NullCorrelationContextAccessor())
    {
    }

    public async Task PublishAsync(
        IEnumerable<OutboxMessage> messages,
        CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
        {
            var domainEvent = serializer.Deserialize(message);

            var correlationId = message.CorrelationId == Guid.Empty
                ? message.Id
                : message.CorrelationId;

            var causationId = message.CausationId ?? message.Id;

            var traceId = message.TraceId ?? Activity.Current?.TraceId.ToString();

            var context = new CorrelationContext(
                correlationId,
                causationId,
                traceId);

            using (correlationContextAccessor.BeginScope(context))
            {
                await mediator.PublishAsync(
                    domainEvent,
                    cancellationToken);
            }
        }
    }

    // ✅ Null Object برای تست
    private sealed class NullCorrelationContextAccessor : ICorrelationContextAccessor
    {
        public CorrelationContext? Current => null;

        public IDisposable BeginScope(CorrelationContext context)
        {
            return NullScope.Instance;
        }

        public void Set(CorrelationContext context)
        {
            // intentionally no-op
        }

        public void Clear()
        {
            // intentionally no-op
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
                // intentionally no-op
            }
        }
    }
}
