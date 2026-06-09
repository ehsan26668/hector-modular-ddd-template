# ADR 0031: Event Schema Evolution Strategy

## Status

Proposed

## Context

The system persists domain events using the Transactional Outbox pattern.

Outbox messages are serialized and stored in the database before being published by the outbox processor. Because of this, events may remain stored for long periods of time.

Over the lifetime of the system, event schemas may evolve due to:

- new business requirements
- additional fields
- changes in data structure
- refactoring of domain models

If event schemas change without a defined evolution strategy, previously stored events may become impossible to deserialize or process correctly.

This is especially problematic for:

- replaying events
- reprocessing failed outbox messages
- migrating systems
- debugging historical data

To ensure long-term stability of the event system, the architecture must define rules for how event schemas evolve over time.

## Decision

The system will adopt a **forward-compatible schema evolution strategy** for all serialized events.

Events stored in the outbox must remain readable even after the codebase evolves.

### Forward Compatibility

Consumers must be able to deserialize older event versions.

Rules:

- New fields must be optional.
- Missing fields should be handled with default values.
- Deserialization must tolerate unknown properties.

Example:

Version 1:

    ProjectCreatedIntegrationEvent
    {
        ProjectId
        Name
    }

Version 2:

    ProjectCreatedIntegrationEvent
    {
        ProjectId
        Name
        CreatedBy
    }

Older serialized events without the `CreatedBy` field must still deserialize successfully.

### Backward Compatibility

When possible, producers should avoid breaking older consumers.

Breaking changes require introducing a new event version.

Example:

    ProjectCreatedIntegrationEventV2

The previous version should remain supported until consumers migrate.

### Serialization Rules

All events stored in the outbox will use a stable JSON serialization format.

Serialization requirements:

- Events must be immutable.
- Events must contain only data.
- Property names must remain stable.
- Property renaming is considered a breaking change.

Unknown fields should be ignored during deserialization.

### Event Type Resolution

Event types will be resolved dynamically during deserialization using the event type metadata stored in the outbox message.

If a previously stored event references a type that has moved or been renamed, compatibility mapping may be introduced in the event type resolver.

### Long-Term Storage Considerations

Outbox messages are not intended to be stored indefinitely. A cleanup policy is defined in a separate ADR.

However, events may remain in the database for operational or diagnostic purposes. Therefore, schema evolution must not break deserialization of historical events.

## Consequences

Positive:

- Enables long-term readability of stored events.
- Prevents deserialization failures when processing historical messages.
- Supports safe evolution of event contracts.
- Aligns the architecture with best practices in event-driven systems.

Negative:

- Requires careful planning when evolving event schemas.
- Old event versions may need to be supported for extended periods.
- Additional compatibility logic may be required in the event type resolver.
