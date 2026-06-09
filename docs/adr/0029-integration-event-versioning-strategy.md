# ADR 0029: Integration Event Versioning Strategy

## Status

Proposed

## Context

Integration events represent communication contracts between modules and potentially external systems.

Examples include:

- ProjectCreatedIntegrationEvent
- UserRegisteredIntegrationEvent
- OrderCompletedIntegrationEvent

Unlike domain events, integration events cross module boundaries and may be consumed by independent components. Because of this, integration events must evolve carefully.

Over time, event schemas may change due to:

- adding new fields
- renaming fields
- removing properties
- changing event structure
- supporting new consumers

Without a versioning strategy, these changes can break existing consumers and introduce tight coupling between producers and consumers.

To support long-term system evolution, integration events must include explicit version information and follow compatibility rules.

## Decision

Integration events will include an explicit **event version**.

Each integration event represents a versioned contract.

Example:

    public sealed record ProjectCreatedIntegrationEvent(
        Guid ProjectId,
        string Name
    ) : IIntegrationEvent
    {
        public int Version => 1;
    }

All integration events will implement a common marker interface.

Example:

    public interface IIntegrationEvent
    {
        int Version { get; }
    }

Versioning rules:

1. New optional fields may be added without increasing the version.
2. Breaking changes require introducing a new version.
3. Old versions should remain supported for a reasonable time window.
4. Producers should avoid removing fields used by existing consumers.
5. Consumers should ignore unknown fields when possible.

Example evolution:

Version 1:

    ProjectCreatedIntegrationEvent
    {
        ProjectId
        Name
    }

Version 2:

    ProjectCreatedIntegrationEventV2
    {
        ProjectId
        Name
        CreatedBy
    }

Multiple versions may coexist in the system while consumers migrate.

Integration events should remain immutable data contracts and must not contain business logic.

Version information may also be included in event metadata when events are transported through message brokers.

## Consequences

Positive:

- Enables safe evolution of integration event contracts.
- Prevents breaking existing consumers.
- Supports long-lived event streams.
- Aligns the architecture with common event-driven system practices.
- Improves compatibility with external messaging infrastructure.

Negative:

- Requires managing multiple versions of events.
- Increases maintenance overhead for event contracts.
- Developers must carefully plan schema evolution.
