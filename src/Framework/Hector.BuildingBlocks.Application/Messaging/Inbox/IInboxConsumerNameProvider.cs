namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public interface IInboxConsumerNameProvider
{
    string ConsumerName { get; }
}