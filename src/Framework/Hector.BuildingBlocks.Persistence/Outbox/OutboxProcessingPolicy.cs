using System.Linq.Expressions;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public static class OutboxProcessingPolicy
{
    public static bool IsProcessable(OutboxMessage message, OutboxOptions options)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(options);

        // ADR-0034: Poisoned messages must never be selected.
        return !message.IsPoisoned &&
               message.RetryCount < options.MaxRetryCount;
    }

    public static Expression<Func<OutboxMessage, bool>> IsProcessableExpression(
        OutboxOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // ADR-0034: Poisoned messages must never be selected.
        return message =>
            !message.IsPoisoned &&
            message.RetryCount < options.MaxRetryCount;
    }
}
