# ADR 0026: Define Event Serialization Strategy

## Status

Accepted

## Context

ADR‑0021 introduced the Transactional Outbox pattern to persist domain events in the `OutboxMessages` table.

Domain events are serialized and stored as JSON payloads together with their .NET type information so that they can later be reconstructed and published by the Outbox Processor.

The current implementation stores:

`Type`

`Content`

Where:

- Type contains the .NET type name of the event
- Content contains the serialized JSON payload

During outbox processing, the event is reconstructed using:

```text
Type.GetType(…)

JsonSerializer.Deserialize(…)
```

While this approach works for basic scenarios, long‑running systems must address additional concerns:

- event schema evolution
- backward compatibility
- event versioning
- cross‑service compatibility
- long‑term storage of serialized events

If event types evolve over time, previously stored events may fail to deserialize or may produce inconsistent results.

Therefore, a clear and stable serialization strategy is required.

## Decision

Domain events stored in the Outbox will be serialized using JSON with explicit metadata describing the event type.

Each outbox message will contain:

`Id`
`Type`
`Content`
`OccurredOn`
`ProcessedOn`

Serialization rules:

1. Events are serialized using `System.Text.Json`.
2. The *Type* field stores the assembly-qualified type name of the event.
3. The *Content* field stores the serialized JSON payload of the event.
4. Event types must remain backward compatible when possible.
5. Event properties should be additive rather than breaking.

Event reconstruction during publishing will follow this process:

1. Resolve the event type using the type resolver.
2. Deserialize the JSON payload into the resolved event type.
3. Publish the event through the mediator.

Example stored record:

```text
Type:

Hector.Modules.Projects.Domain.ProjectCreatedDomainEvent,

Hector.Modules.Projects.Domain

Content:

{

“ProjectId”: “6e0b8a6e-45a8-4d64-92e0-8c0d44d8a55a”,

“OccurredOn”: “2026-05-12T10:23:44Z”

}
```

To improve performance and reliability, type resolution may use caching to avoid repeated reflection lookups.

Future enhancements may include:

- event version identifiers
- schema evolution support
- alternative serialization formats (e.g., MessagePack, Avro)
- contract-based event publishing for external integrations

## Consequences

Positive:

- ensures consistent event reconstruction
- provides a clear serialization standard
- supports long‑term storage of domain events
- enables compatibility with the transactional outbox mechanism
- allows future extensibility for event versioning

Negative:

- tight coupling to .NET type names
- potential issues if event types are renamed or moved
- requires careful management of event schema evolution
