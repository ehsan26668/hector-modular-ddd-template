# ADR 0029: Integration Event Versioning Strategy

## Status

Accepted

## Context

The project uses integration events to publish domain changes through the outbox pattern. The implementation does **not** place a `Version` property on `IIntegrationEvent` payloads. Instead, versioning is handled as **event contract metadata** via `OutboxEventAttribute` and persisted in `OutboxMessage.Version`.

This means the effective identity of an integration event contract is:

`EventName:v{Version}`

The runtime resolves the concrete event type using the contract name and version, while the serialized payload remains version-free.

## Decision

We standardize integration event versioning as metadata-driven contract versioning:

1. `IIntegrationEvent` must remain version-free.
2. Event version is declared on the event type via `OutboxEventAttribute(Name, Version)`.
3. `OutboxMessage` persists both the contract name and version.
4. Serialization is performed using the concrete CLR type.
5. Deserialization resolves the CLR type from `(Type, Version)` using `IOutboxEventTypeResolver`.
6. Domain event handlers publish integration events through `IIntegrationEventBus` without manually managing transport metadata.

## Consequences

### Positive

- Keeps integration event payloads clean and stable.
- Avoids leaking transport/version concerns into consumer-facing contracts.
- Enables multiple versions of the same event contract to coexist safely.
- Makes contract evolution explicit and traceable.
- Aligns with the existing outbox implementation and tests.

### Negative

- Requires a resolver and attribute scan at startup.
- Contract changes must be coordinated carefully to avoid version collisions.

## Implementation Notes

### Event declaration

Integration/domain event contracts define metadata with `OutboxEventAttribute`:

```csharp
[OutboxEvent("projects.project-created", 1)]
public sealed record ProjectCreatedIntegrationEvent(
    Guid MessageId,
    Guid ProjectId,
    string Name)
    : IIntegrationEvent, IInboxMessage;
```

### Outbox message model

`OutboxMessage` stores the event contract metadata:

- `Type`
- `Version`
- `Content`
- processing state fields

### Publishing flow

`OutboxIntegrationEventBus` converts an `IIntegrationEvent` to an `OutboxMessage` via `IOutboxMessageFactory` and stores it in the database.

### Processing flow

`OutboxProcessor` locks eligible messages, sends them to `OutboxPublisher`, and updates processing state.

### Serialization flow

`SystemTextJsonOutboxEventSerializer`:

- reads metadata from `IOutboxEventTypeResolver`
- serializes the notification as JSON
- resolves the target CLR type from stored `(Type, Version)`
- deserializes back to `INotification`

### Type resolution

`AttributedOutboxEventTypeResolver` scans assemblies for `INotification` types decorated with `OutboxEventAttribute` and builds a contract map keyed by `(Name, Version)`.

## Test Strategy

The contract is protected by serialization and publisher tests, including:

- type-name resolution
- serialization and deserialization
- unknown contract failures
- ordered publishing
- failure short-circuiting

## Related Decisions

- ADR-0021: Outbox Pattern Adoption
- ADR-0030: Integration Event Consumer Contract Handling
- ADR-0031: Event Schema Evolution Strategy

## Notes

If a future event change is backward-incompatible, the correct action is to introduce a **new contract version** through `OutboxEventAttribute`, not to add a `Version` property to the payload.
