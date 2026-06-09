using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hector.BuildingBlocks.Persistence.Outbox;

internal sealed class OutboxCleaner(
    HectorDbContext context,
    IOptions<OutboxOptions> options) : IOutboxCleaner
{
    public async Task CleanupAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow - options.Value.RetentionPeriod;

        var message = await context.OutboxMessages
            .Where(x =>
                x.ProcessedOn != null &&
                x.ProcessedOn < cutoff)
            .OrderBy(x => x.ProcessedOn)
            .Take(options.Value.CleanupBatchSize)
            .ToListAsync(cancellationToken);

        if (message.Count == 0) return;

        context.OutboxMessages.RemoveRange(message);

        await context.SaveChangesAsync(cancellationToken);
    }
}