using Hector.BuildingBlocks.Application.Messaging.Inbox;

namespace Hector.BuildingBlocks.Application.Messaging;

public interface IIntegrationEventBus
{
    Task PublishAsync(
        IInboxMessage message,
        CancellationToken cancellationToken = default);
}