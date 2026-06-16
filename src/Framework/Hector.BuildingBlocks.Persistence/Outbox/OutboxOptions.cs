namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxOptions
{
    public int BatchSize { get; init; } = 20;

    public int MaxRetryCount { get; init; } = 5;

    public TimeSpan LockDuration { get; init; } = TimeSpan.FromMinutes(2);

    public TimeSpan RetentionPeriod { get; init; } = TimeSpan.FromDays(7);

    public int CleanupBatchSize { get; init; } = 100;

    public TimeSpan InitialRetryDelay { get; init; } = TimeSpan.FromSeconds(5);

    public TimeSpan MaxRetryDelay { get; init; } = TimeSpan.FromMinutes(10);

    public int MaxErrorLength { get; init; } = 2000;
}
