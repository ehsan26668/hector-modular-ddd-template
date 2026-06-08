namespace Hector.BuildingBlocks.Persistence.Inbox;

public sealed class InboxMessage
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public string Consumer { get; set; } = string.Empty;
    public DateTime ProcessedOn { get; set; }
}