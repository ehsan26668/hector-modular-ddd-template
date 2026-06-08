namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public interface IInboxStore
{
    Task<bool> ExistsAsync(Guid messageId, string consumer, CancellationToken cancellationToken = default);

    Task StoreAsync(Guid messageId, string consumer, CancellationToken cancellationToken = default);
}