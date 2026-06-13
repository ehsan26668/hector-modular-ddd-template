using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public interface IOutboxEventTypeResolver
{
    Type Resolve(string typeName, int version);

    OutboxEventMetadata GetMetadata(Type eventType);
}