using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Application.Messaging;

public interface IIntegrationEvent : INotification
{
    Guid MessageId { get; }
    Guid CorrelationId { get; }
    Guid? CausationId { get; }
    string? TraceId { get; }
}