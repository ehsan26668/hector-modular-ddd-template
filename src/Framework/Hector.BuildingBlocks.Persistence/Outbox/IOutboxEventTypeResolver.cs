using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public interface IOutboxEventTypeResolver
{
    Type Resolve(string eventName, int version);

    OutboxEventMetadata GetMetadata(Type eventType);
}