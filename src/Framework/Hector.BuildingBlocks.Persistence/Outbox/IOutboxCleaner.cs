namespace Hector.BuildingBlocks.Persistence.Outbox;

internal interface IOutboxCleaner
{
    Task CleanupAsync(CancellationToken cancellationToken);
}