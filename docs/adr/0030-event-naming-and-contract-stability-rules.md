# ADR 0030: Event Naming and Contract Stability Rules

## Status

Implemented  
Date: 2026-06-15

## Context

The system relies heavily on events for communication between modules and for future integration with external systems.

Two types of events exist in the architecture:

- Domain Events
- Integration Events

Domain events represent internal state changes within a bounded context. Integration events represent stable contracts intended for cross-module communication and potentially external consumers.

As the number of events grows, inconsistent naming conventions and poorly managed contracts can lead to:

- confusion between domain and integration events
- breaking changes that impact consumers
- duplicated or ambiguous events
- tightly coupled modules

To maintain long-term maintainability and clarity, the system requires standardized rules for naming events and managing event contracts.

## Decision

The system will adopt strict conventions for event naming and contract stability.

### Event Naming Rules

All events must follow the **past-tense naming convention**, representing something that has already occurred.

Examples:

```csharp
    ProjectCreatedDomainEvent
    UserRegisteredDomainEvent
    OrderCompletedDomainEvent
```

Integration events follow a similar naming convention but omit the "Domain" suffix.

Examples:

```csharp
    ProjectCreatedIntegrationEvent
    UserRegisteredIntegrationEvent
    OrderCompletedIntegrationEvent
```

Rules:

- Event names MUST use past tense.
- Domain events MUST end with "DomainEvent".
- Integration events MUST end with "IntegrationEvent".
- Event names MUST describe a business fact, not an action.

Incorrect examples:

```csharp
    CreateProjectEvent
    ProjectCreateEvent
    DoSomethingEvent
```

Correct examples:

```csharp
    ProjectCreatedDomainEvent
    ProjectCreatedIntegrationEvent
```

### Event Contract Location

Integration event contracts must live in a dedicated **Contracts project** of each module.

Example structure:

```text
    Modules
        Projects
            Contracts
                Events
                    ProjectCreatedIntegrationEvent.cs
```

Application and infrastructure layers may reference the contracts project, but domain logic must not depend on integration events.

### Example

```csharp
A typical integration event contract following the established rules (C#):

    [OutboxEvent("projects.project-created", 1)]
    public sealed record ProjectCreatedIntegrationEvent(
        Guid MessageId,
        Guid ProjectId,
        string Name
    ) : IIntegrationEvent;
```

Characteristics of this contract:

- The event name follows the past-tense convention.
- The event is immutable (record type).
- The contract contains only data.
- The `OutboxEvent` attribute provides the canonical event name and version used by the outbox infrastructure.
- The event does not contain consumer or infrastructure-specific concerns.

### Contract Stability Rules

Integration events represent public contracts and must remain stable.

Rules:

- Integration events MUST be immutable.
- Integration events MUST only contain data.
- Integration events MUST NOT include domain logic.
- Breaking changes MUST introduce a new version of the event.
- Fields SHOULD NOT be removed once published.

Compatible changes include:

- adding optional fields
- adding metadata
- extending consumers

Breaking changes include:

- removing properties
- renaming properties
- changing property types

### Event Granularity

Events should represent meaningful business facts rather than low-level technical details.

Example:

```csharp
    ProjectCreatedIntegrationEvent
```

instead of

```csharp
    ProjectRowInsertedEvent
```

Events should reflect the ubiquitous language of the domain.

### Event Ownership

Each module owns its own integration event contracts.

Other modules may subscribe to these events but must not modify them.

### Contract Purity

Integration event contracts must remain independent from messaging infrastructure concerns.

Rules:

- Integration events MUST NOT implement inbox-related interfaces.
- Integration events MUST NOT contain consumer identifiers.
- Integration events MUST NOT contain transport-specific metadata.

Examples of forbidden patterns:

```csharp
    class ProjectCreatedIntegrationEvent : IInboxMessage
```

or

```csharp
    public string Consumer { get; }
```

Consumer identity and idempotency mechanisms belong to the subscriber side and must be handled by the inbox infrastructure.

This rule ensures that integration event contracts remain pure data contracts and reusable across different messaging infrastructures.

## Consequences

Positive:

- Consistent naming across the entire system.
- Clear distinction between domain events and integration events.
- Stable integration contracts for cross-module communication.
- Reduced risk of breaking consumers when evolving the system.
- Better alignment with domain-driven design and event-driven architecture principles.

Negative:

- Developers must follow strict naming conventions.
- Changes to integration events require careful version management.
- Additional governance may be required as the number of events grows.
