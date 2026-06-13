namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public interface IInboxStore
{
    Task<bool> TryStoreAsync(
        Guid messageId,
        string consumer,
        CancellationToken cancellationToken = default);
}