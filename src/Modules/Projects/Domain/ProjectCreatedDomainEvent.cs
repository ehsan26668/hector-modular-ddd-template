using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.Modules.Projects.Domain;

[OutboxEvent("projects.project-created", 1)]
public sealed record ProjectCreatedDomainEvent(
    ProjectId ProjectId,
    string Name) : DomainEventBase;
