namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public sealed class StaticInboxConsumerNameProvider(
    string consumerName)
    : IInboxConsumerNameProvider
{
    public string ConsumerName { get; } = consumerName;
}