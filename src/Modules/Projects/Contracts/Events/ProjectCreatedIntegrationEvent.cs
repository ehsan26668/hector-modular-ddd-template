using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Inbox;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.Modules.Projects.Contracts.Events;

[OutboxEvent("projects.project-created", 1)]
public sealed record ProjectCreatedIntegrationEvent(
    Guid MessageId,
    Guid ProjectId,
    string Name)
    : IIntegrationEvent, IInboxMessage
{
    public string Consumer => "Projects";
}