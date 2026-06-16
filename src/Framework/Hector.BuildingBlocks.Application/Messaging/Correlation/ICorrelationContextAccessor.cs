namespace Hector.BuildingBlocks.Application.Messaging.Correlation;

public interface ICorrelationContextAccessor
{
    CorrelationContext? Current { get; }
    void Set(CorrelationContext context);
    void Clear();
}