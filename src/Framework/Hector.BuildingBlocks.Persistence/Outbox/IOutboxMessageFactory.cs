using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public interface IOutboxMessageFactory
{
    OutboxMessage Create(IIntegrationEvent integrationEvent);
}