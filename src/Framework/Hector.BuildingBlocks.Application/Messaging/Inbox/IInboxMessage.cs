namespace Hector.BuildingBlocks.Application.Messaging.Inbox;

public interface IInboxMessage
{
    Guid MessageId { get; }
    string Consumer { get; }
}