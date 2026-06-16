using System.Diagnostics;
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
    private static readonly ActivitySource ActivitySource = new("Hector.Outbox");

    public async Task ProcessAsync(CancellationToken cancellationToken)
    {
        var outboxOptions = options.Value;

        var now = DateTime.UtcNow;

        var lockId = Guid.NewGuid();

        var lockedUntil = now.Add(outboxOptions.LockDuration);

        var messageIds = await dbContext.OutboxMessages
            .Where(m =>
                m.ProcessedOn == null &&
                m.DeadLetteredOn == null &&
                m.RetryCount < outboxOptions.MaxRetryCount &&
                (m.LockedUntil == null || m.LockedUntil < now))
            .OrderBy(m => m.OccurredOn)
            .ThenBy(m => m.Id)
            .Take(outboxOptions.BatchSize)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (messageIds.Count == 0)
            return;

        var lockedCount = await dbContext.OutboxMessages
            .Where(m =>
                messageIds.Contains(m.Id) &&
                m.ProcessedOn == null &&
                m.DeadLetteredOn == null &&
                m.RetryCount < outboxOptions.MaxRetryCount &&
                (m.LockedUntil == null || m.LockedUntil < now))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(m => m.LockId, lockId)
                    .SetProperty(m => m.LockedUntil, lockedUntil),
                cancellationToken);

        if (lockedCount == 0)
            return;

        var messages = await dbContext.OutboxMessages
            .Where(m =>
                m.LockId == lockId &&
                m.ProcessedOn == null &&
                m.DeadLetteredOn == null)
            .OrderBy(m => m.OccurredOn)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        foreach (var message in messages)
        {
            using var activity = ActivitySource.StartActivity("Outbox.ProcessMessage");

            activity?.SetTag("outbox.message_id", message.Id);
            activity?.SetTag("outbox.type", message.Type);
            activity?.SetTag("outbox.retry_count", message.RetryCount);

            var attemptTime = DateTime.UtcNow;

            try
            {
                await publisher.PublishAsync([message], cancellationToken);

                message.ProcessedOn = attemptTime;
                message.LastAttemptedOn = attemptTime;
                message.Error = null;

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            catch (Exception ex)
            {
                message.RetryCount++;

                message.LastAttemptedOn = attemptTime;

                message.Error = Truncate(ex.ToString(), outboxOptions.MaxErrorLength);

                if (message.RetryCount >= outboxOptions.MaxRetryCount)
                {
                    message.DeadLetteredOn = attemptTime;
                    message.DeadLetterReason = message.Error;

                    logger.LogError(
                        ex,
                        "Outbox message {MessageId} moved to dead letter after {RetryCount} retries",
                        message.Id,
                        message.RetryCount);
                }
                else
                {
                    var delay = CalculateRetryDelay(
                        message.RetryCount,
                        outboxOptions.InitialRetryDelay,
                        outboxOptions.MaxRetryDelay);

                    message.LockedUntil = attemptTime.Add(delay);

                    logger.LogWarning(
                        ex,
                        "Retrying outbox message {MessageId}. Retry {RetryCount}. Next attempt in {Delay}",
                        message.Id,
                        message.RetryCount,
                        delay);
                }

                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);
            }
            finally
            {
                message.LockId = null;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static TimeSpan CalculateRetryDelay(
        int retryCount,
        TimeSpan initialDelay,
        TimeSpan maxDelay)
    {
        var delayMs = initialDelay.TotalMilliseconds * Math.Pow(2, retryCount - 1);

        var capped = Math.Min(delayMs, maxDelay.TotalMilliseconds);

        return TimeSpan.FromMilliseconds(capped);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        return value[..maxLength];
    }
}
