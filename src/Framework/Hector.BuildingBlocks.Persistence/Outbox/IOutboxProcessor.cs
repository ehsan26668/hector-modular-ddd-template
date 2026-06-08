namespace Hector.BuildingBlocks.Persistence.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync(CancellationToken cancellationToken);
}