using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public interface IOutboxEventSerializer
{
    string GetTypeName(INotification notification);

    string Serialize(INotification notification);

    INotification Deserialize(OutboxMessage message);
}