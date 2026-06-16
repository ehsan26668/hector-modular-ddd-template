namespace Hector.BuildingBlocks.Application.Messaging.Correlation;

public interface ICorrelationContextAccessor
{
    CorrelationContext? Current { get; }
    IDisposable BeginScope(CorrelationContext context);
    void Set(CorrelationContext context);
    void Clear();
}