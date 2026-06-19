using Hector.BuildingBlocks.Application.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.Inbox;

public sealed class EfCoreInboxStore(HectorDbContext context) : IInboxStore
{
    public async Task<bool> TryStoreAsync(
        Guid messageId,
        string consumer,
        CancellationToken cancellationToken = default)
    {
        var message = new InboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            Consumer = consumer,
            ProcessedOn = DateTime.UtcNow
        };

        context.Set<InboxMessage>().Add(message);

        try
        {
            await context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);

            return true;
        }
        catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
        {
            context.Entry(message).State = EntityState.Detached;
            return false;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message;

        if (message is null)
        {
            return false;
        }

        return message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)
            || message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_InboxMessages_MessageId_Consumer", StringComparison.OrdinalIgnoreCase);
    }
}
