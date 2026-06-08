using Hector.BuildingBlocks.Application.Messaging.Inbox;

namespace Hector.Modules.Projects.Contracts.Events;

public sealed record ProjectCreatedIntegrationEvent(
    Guid MessageId,
    Guid ProjectId,
    string Name)
    : IInboxMessage
{
    public string Consumer => "Projects";
}