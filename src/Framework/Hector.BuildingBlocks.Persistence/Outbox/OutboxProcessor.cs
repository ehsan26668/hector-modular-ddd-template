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
        var now = DateTime.UtcNow;
        var lockId = Guid.NewGuid();
        var lockedUntil = now.Add(options.Value.LockDuration);

        var messageIds = await dbContext.OutboxMessages
            .Where(m =>
                m.ProcessedOn == null &&
                m.RetryCount < options.Value.MaxRetryCount &&
                (m.LockedUntil == null || m.LockedUntil < now))
            .OrderBy(m => m.OccurredOn)
            .Take(options.Value.BatchSize)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (messageIds.Count == 0) return;

        await dbContext.OutboxMessages
            .Where(m =>
                messageIds.Contains(m.Id) &&
                m.ProcessedOn == null &&
                m.RetryCount < options.Value.MaxRetryCount &&
                (m.LockedUntil == null || m.LockedUntil < now))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.LockId, lockId)
                    .SetProperty(m => m.LockedUntil, lockedUntil),
                cancellationToken);

        var messages = await dbContext.OutboxMessages
            .Where(m => m.LockId == lockId)
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
            foreach (var message in messages)
            {
                message.RetryCount++;
                message.LastAttemptedOn = DateTime.UtcNow;
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

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await publisher.PublishAsync([message], cancellationToken);

            message.ProcessedOn = DateTime.UtcNow;
            message.LastAttemptedOn = DateTime.UtcNow;
            message.Error = null;
            message.LockId = null;
            message.LockedUntil = null;

            logger.LogInformation(
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

            logger.LogError(
                ex,
                "Failed to process outbox message {OutboxMessageId}. RetryCount: {RetryCount}",
                message.Id,
                message.RetryCount);
        }
    }
}
