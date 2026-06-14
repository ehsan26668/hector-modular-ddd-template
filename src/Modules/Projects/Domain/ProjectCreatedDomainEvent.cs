using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.Modules.Projects.Domain;

public sealed record ProjectCreatedDomainEvent(
    ProjectId ProjectId,
    string Name) : DomainEventBase;
