namespace Hector.BuildingBlocks.Application.Messaging.Correlation;

public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext?> Context = new();

    public CorrelationContext? Current => Context.Value;

    public IDisposable BeginScope(CorrelationContext context)
    {
        var previous = Context.Value;

        Context.Value = context;

        return new Scope(previous);
    }

    public void Clear()
    {
        Context.Value = null;
    }

    public void Set(CorrelationContext context)
    {
        Context.Value = context;
    }

    private sealed class Scope(
        CorrelationContext? previous)
        : IDisposable
    {
        public void Dispose()
        {
            Context.Value = previous;
        }
    }
}