namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    public string Content { get; set; } = string.Empty;

    public DateTime OccurredOn { get; set; }

    public Guid CorrelationId { get; set; }

    public Guid? CausationId { get; set; }

    public string? TraceId { get; set; }

    public string Producer { get; set; } = string.Empty;

    public DateTime? ProcessedOn { get; set; }

    public int RetryCount { get; set; }

    public string? Error { get; set; }

    public DateTime? LastAttemptedOn { get; set; }

    public DateTime? LockedUntil { get; set; }

    public Guid? LockId { get; set; }

    public DateTime? FailedOn { get; set; }

    public string? FailureReason { get; set; }

    public bool IsPoisoned { get; set; }

    public void MarkAsPoison(string failureReason, DateTime utcNow)
    {
        IsPoisoned = true;
        FailedOn = utcNow;
        FailureReason = failureReason;
        LockedUntil = null;
    }
}
