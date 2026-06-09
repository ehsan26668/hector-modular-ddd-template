using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxProcessor : IOutboxProcessor
{
    private readonly HectorDbContext _dbContext;
    private readonly IOutboxPublisher _publisher;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxOptions _options;

    public OutboxProcessor(
        HectorDbContext dbContext,
        IOutboxPublisher publisher,
        ILogger<OutboxProcessor> logger,
        IOptions<OutboxOptions> options)
    {
        _dbContext = dbContext;
        _publisher = publisher;
        _logger = logger;
        _options = options.Value;
    }

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var lockId = Guid.NewGuid();
        var lockedUntil = now.Add(_options.LockDuration);

        var messageIds = await _dbContext.OutboxMessages
            .Where(m =>
                m.ProcessedOn == null &&
                m.RetryCount < _options.MaxRetryCount &&
                (m.LockedUntil == null || m.LockedUntil < now))
            .OrderBy(m => m.OccurredOn)
            .Take(_options.BatchSize)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (messageIds.Count == 0)
        {
            return;
        }

        await _dbContext.OutboxMessages
            .Where(m =>
                messageIds.Contains(m.Id) &&
                m.ProcessedOn == null &&
                m.RetryCount < _options.MaxRetryCount &&
                (m.LockedUntil == null || m.LockedUntil < now))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.LockId, lockId)
                    .SetProperty(m => m.LockedUntil, lockedUntil),
                cancellationToken);

        var messages = await _dbContext.OutboxMessages
            .Where(m => m.LockId == lockId)
            .OrderBy(m => m.OccurredOn)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            await ProcessMessageAsync(message, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await _publisher.PublishAsync([message], cancellationToken);

            message.ProcessedOn = DateTime.UtcNow;
            message.LastAttemptedOn = DateTime.UtcNow;
            message.Error = null;
            message.LockId = null;
            message.LockedUntil = null;

            _logger.LogInformation(
                "Outbox message {OutboxMessageId} processed successfully",
                message.Id);
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.LastAttemptedOn = DateTime.UtcNow;
            message.Error = ex.Message;
            message.LockId = null;
            message.LockedUntil = null;

            _logger.LogError(
                ex,
                "Failed to process outbox message {OutboxMessageId}. RetryCount: {RetryCount}",
                message.Id,
                message.RetryCount);
        }
    }
}
