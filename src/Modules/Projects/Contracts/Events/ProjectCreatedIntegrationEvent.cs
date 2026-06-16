using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.Modules.Projects.Contracts.Events;

[OutboxEvent("projects.project-created", 1)]
public sealed record ProjectCreatedIntegrationEvent(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    string? TraceId,
    Guid ProjectId,
    string Name)
    : IIntegrationEvent;