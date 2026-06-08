using System.Text.Json;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Microsoft.Extensions.Logging;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxPublisher : IOutboxPublisher
{
    private readonly IMediator _mediator;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly IOutboxEventTypeResolver _typeResolver;

    public OutboxPublisher(
        IMediator mediator,
        ILogger<OutboxPublisher> logger,
        IOutboxEventTypeResolver typeResolver)
    {
        _mediator = mediator;
        _logger = logger;
        _typeResolver = typeResolver;
    }

    public async Task PublishAsync(
        IEnumerable<OutboxMessage> messages,
        CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
        {
            var type = _typeResolver.Resolve(message.Type);

            if (type is null)
            {
                _logger.LogError(
                    "Outbox message {MessageId} skipped. Type '{Type}' could not be resolved.",
                    message.Id,
                    message.Type);

                throw new InvalidOperationException(
                    $"Outbox message type '{message.Type}' could not be resolved.");
            }

            INotification? domainEvent;

            try
            {
                domainEvent = JsonSerializer.Deserialize(
                    message.Content,
                    type) as INotification;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to deserialize outbox message {MessageId}",
                    message.Id);

                throw;
            }

            if (domainEvent is null)
            {
                throw new InvalidOperationException(
                    $"Outbox message {message.Id} deserialized to null.");
            }

            await _mediator.PublishAsync(domainEvent, cancellationToken);
        }
    }
}
