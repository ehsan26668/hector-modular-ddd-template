using Hector.BuildingBlocks.Application.Messaging;
using Microsoft.Extensions.Logging;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxPublisher : IOutboxPublisher
{
    private readonly IMediator _mediator;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly IOutboxEventSerializer _serializer;

    public OutboxPublisher(
        IMediator mediator,
        ILogger<OutboxPublisher> logger,
        IOutboxEventSerializer serializer)
    {
        _mediator = mediator;
        _logger = logger;
        _serializer = serializer;
    }

    public async Task PublishAsync(
        IEnumerable<OutboxMessage> messages,
        CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
        {
            try
            {
                var domainEvent = _serializer.Deserialize(message);

                await _mediator.PublishAsync(domainEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish outbox message {MessageId}",
                    message.Id);

                throw;
            }
        }
    }
}
