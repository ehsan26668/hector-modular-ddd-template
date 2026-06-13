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

        var messages = await context.OutboxMessages
            .Where(message =>
                message.ProcessedOn != null &&
                message.ProcessedOn < cutoff)
            .OrderBy(message => message.ProcessedOn)
            .Take(options.Value.CleanupBatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0) return;

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