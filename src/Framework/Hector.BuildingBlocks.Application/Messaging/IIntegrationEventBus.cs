namespace Hector.BuildingBlocks.Application.Messaging;

public interface IIntegrationEventBus
{
    Task PublishAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);
}