# ADR 0026: Define Event Serialization Strategy

## Status

Implemented

Implemented on: 2026-06-14

## Context

ADR‑0021 introduced the Transactional Outbox pattern to persist domain events in the `OutboxMessages` table.

Domain events are serialized and stored as JSON payloads together with explicit contract metadata so that they can later be reconstructed and published by the Outbox Processor.

The current strategy stores:

- `Type`
- `Version`
- `Content`

Where:

- `Type` contains the stable logical event name
- `Version` contains the event contract version
- `Content` contains the serialized JSON payload

Long-running systems must address additional concerns:

- event schema evolution
- backward compatibility
- event versioning
- cross-service compatibility
- long-term storage of serialized events

Therefore, a clear and stable serialization strategy is required.

## Decision

Domain events stored in the Outbox will be serialized using JSON with explicit metadata describing the event contract.

Each outbox message will contain:

- `Id`
- `Type`
- `Version`
- `Content`
- `OccurredOn`
- `ProcessedOn`

Serialization rules:

1. Events are serialized using `System.Text.Json`.
2. The `Type` field stores the stable logical event name instead of the .NET full type name.
3. The `Version` field stores the contract version as an integer starting at `1`.
4. The `Content` field stores the serialized JSON payload of the event.
5. Each domain event must declare its contract explicitly using `OutboxEventAttribute`.
6. The mapping between logical contracts and CLR types is managed by `AttributedOutboxEventTypeResolver`.
7. Contract identity is defined by the pair `(Name, Version)`.
8. The same logical event name may exist in multiple versions simultaneously.
9. The combination `(Name, Version)` must be unique across all registered event types.
10. Event properties should remain backward compatible when possible and evolve additively where feasible.

Event reconstruction during publishing follows this process:

1. Read `Type`, `Version`, and `Content` from the outbox message.
2. Resolve the CLR event type using `IOutboxEventTypeResolver`.
3. Deserialize the JSON payload into the resolved event type.
4. Publish the reconstructed event through the mediator.

Example stored record:

```text
Type: projects.project-created
Version: 1
Content:
{
  "ProjectId": "6e0b8a6e-45a8-4d64-92e0-8c0d44d8a55a",
  "OccurredOn": "2026-05-12T10:23:44Z"
}
```

To improve reliability and enforce contract stability, the implementation is protected by three layers:

1. Explicit contract declaration using `OutboxEventAttribute`
2. Architecture tests to prevent duplicate `(Name, Version)` registrations
3. Snapshot tests to detect accidental event contract changes

Future enhancements may include:

- event upcasters
- richer schema evolution support
- alternative serialization formats
- contract-based event publishing for external integrations

## Consequences

### Positive

- ensures stable event reconstruction independent of CLR type names
- provides a clear serialization standard
- supports long-term storage of domain events
- enables side-by-side support for multiple event versions
- improves refactoring safety by decoupling persisted contracts from .NET type names
- allows architectural enforcement of contract uniqueness and stability

### Negative

- requires explicit contract registration on every persisted domain event
- introduces contract governance overhead for versioning and snapshots
- schema-breaking changes still require deliberate migration or upcasting strategy
