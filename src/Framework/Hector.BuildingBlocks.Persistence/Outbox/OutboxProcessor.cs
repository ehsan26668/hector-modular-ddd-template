using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxProcessor(
    HectorDbContext dbContext,
    IOutboxPublisher publisher,
    ILogger<OutboxProcessor> logger,
    IOptions<OutboxOptions> options)
    : IOutboxProcessor
{
    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var outboxOptions = options.Value;
        var now = DateTime.UtcNow;
        var lockId = Guid.NewGuid();
        var lockedUntil = now.Add(outboxOptions.LockDuration);

        var messageIds = await dbContext.OutboxMessages
            .Where(m =>
                m.ProcessedOn == null &&
                m.RetryCount < outboxOptions.MaxRetryCount &&
                (m.LockedUntil == null || m.LockedUntil < now))
            .OrderBy(m => m.OccurredOn)
            .Take(outboxOptions.BatchSize)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (messageIds.Count == 0) return;

        var lockedCount = await dbContext.OutboxMessages
            .Where(m =>
                messageIds.Contains(m.Id) &&
                m.ProcessedOn == null &&
                m.RetryCount < outboxOptions.MaxRetryCount &&
                (m.LockedUntil == null || m.LockedUntil < now))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.LockId, lockId)
                    .SetProperty(m => m.LockedUntil, lockedUntil),
                cancellationToken);

        if (lockedCount == 0) return;

        var messages = await dbContext.OutboxMessages
            .Where(m =>
                m.LockId == lockId &&
                m.ProcessedOn == null &&
                m.RetryCount < outboxOptions.MaxRetryCount)
            .OrderBy(m => m.OccurredOn)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

        try
        {
            await publisher.PublishAsync(messages, cancellationToken);

            var processedOn = DateTime.UtcNow;

            foreach (var message in messages)
            {
                message.ProcessedOn = processedOn;
                message.LastAttemptedOn = processedOn;
                message.Error = null;
                message.LockId = null;
                message.LockedUntil = null;
            }

            logger.LogInformation(
                "Processed {Count} outbox messages",
                messages.Count);
        }
        catch (Exception ex)
        {
            var attemptedOn = DateTime.UtcNow;

            foreach (var message in messages)
            {
                message.RetryCount++;
                message.LastAttemptedOn = attemptedOn;
                message.Error = ex.Message;
                message.LockId = null;
                message.LockedUntil = null;
            }

            logger.LogError(
                ex,
                "Failed to process outbox batch of {Count} messages",
                messages.Count);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

}
