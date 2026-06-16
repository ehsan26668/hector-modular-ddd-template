namespace Hector.BuildingBlocks.Application.Messaging.Correlation;

public sealed record CorrelationContext(
    Guid CorrelationId,
    Guid? CausationId,
    string? TraceId);