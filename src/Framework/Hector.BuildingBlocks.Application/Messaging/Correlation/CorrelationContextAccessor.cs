namespace Hector.BuildingBlocks.Application.Messaging.Correlation;

public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext?> Context = new();

    public CorrelationContext? Current => Context.Value;

    public void Clear()
    {
        Context.Value = null;
    }

    public void Set(CorrelationContext context)
    {
        Context.Value = context;
    }
}