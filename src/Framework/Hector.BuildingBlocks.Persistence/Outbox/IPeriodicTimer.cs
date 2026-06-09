namespace Hector.BuildingBlocks.Persistence.Outbox;

internal interface IPeriodicTimer
{
    Task<bool> WaitForNextTickAsync(CancellationToken cancellationToken);
}
