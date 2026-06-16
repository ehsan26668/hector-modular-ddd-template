using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hector.BuildingBlocks.Persistence.Outbox;

internal sealed class OutboxCleaner(
    HectorDbContext context,
    IOptions<OutboxOptions> options)
    : IOutboxCleaner
{
    public async Task CleanupAsync(CancellationToken cancellationToken)
    {
        ValidateOptions(options.Value);

        var cutoff = DateTime.UtcNow - options.Value.RetentionPeriod;

        // Eligible messages:
        // 1) Processed messages older than retention
        // 2) Poisoned messages older than retention (based on FailedOn)
        var messages = await context.OutboxMessages
            .Where(message =>
                (message.ProcessedOn != null && message.ProcessedOn < cutoff) ||
                (message.IsPoisoned &&
                 message.FailedOn != null &&
                 message.FailedOn < cutoff))
            .OrderBy(message => message.ProcessedOn ?? message.FailedOn)
            .Take(options.Value.CleanupBatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
            return;

        context.OutboxMessages.RemoveRange(messages);

        await context.SaveChangesAsync(cancellationToken);
    }

    private static void ValidateOptions(OutboxOptions options)
    {
        if (options.RetentionPeriod <= TimeSpan.Zero)
            throw new InvalidOperationException(
                "Outbox retention period must be greater than zero.");

        if (options.CleanupBatchSize <= 0)
            throw new InvalidOperationException(
                "Outbox cleanup batch size must be greater than zero.");
    }
}
