using Hector.BuildingBlocks.Application.Messaging.Inbox;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.Inbox;

public sealed class EfCoreInboxStore : IInboxStore
{
    private readonly DbContext _Context;

    public EfCoreInboxStore(DbContext context)
    {
        _Context = context;
    }

    public async Task<bool> ExistsAsync(
        Guid messageId,
        string consumer,
        CancellationToken cancellationToken = default)
    {
        return await _Context.Set<InboxMessage>()
            .AnyAsync(
                x => x.MessageId == messageId && x.Consumer == consumer,
                cancellationToken);
    }

    public async Task StoreAsync(
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

        _Context.Set<InboxMessage>().Add(message);

        await _Context.SaveChangesAsync(cancellationToken);
    }
}