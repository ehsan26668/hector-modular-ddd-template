# ADR 0027: Domain Event to Integration Event Bridge

## Status

Accepted

## Context

The system already uses Domain Events to represent significant changes within a module.

Examples include:

- ProjectCreatedDomainEvent
- UserRegisteredDomainEvent
- OrderCompletedDomainEvent

Domain events are dispatched internally through the mediator and are primarily intended for:

- enforcing domain invariants
- triggering internal workflows
- coordinating behavior within the same module

However, modules also need to communicate with other modules and external systems. For that purpose, the system defines **Integration Events**, such as:

- ProjectCreatedIntegrationEvent

Integration events represent stable contracts intended for cross-module communication or external messaging infrastructure (e.g., message brokers).

Currently, there is no standardized mechanism that converts domain events into integration events. Without a clear pattern, each module may implement its own mapping logic, leading to:

- duplication
- inconsistent integration boundaries
- tight coupling between modules

To maintain a clean modular architecture and enforce ADR-0039, the system needs a clear separation between internal domain events and externally visible integration events, along with a consistent mechanism to transform one into the other.

## Decision

The system will introduce a **Domain Event → Integration Event Bridge**.

Domain events remain internal to their module. Integration events represent external communication contracts. The bridge is implemented using Application-layer domain event handlers that map domain events to integration events.

The flow is:

1. A domain event is raised inside an aggregate.
2. After successful persistence, the domain event is dispatched in-process inside the same module.
3. An Application-layer domain event handler maps the domain event to an integration event.
4. The handler publishes the integration event through `IIntegrationEventBus`. In accordance with ADR-0039, the producer-side publishing contract is `IIntegrationEventBus`, and it accepts integration event contracts only. It must not expose consumer-side Inbox abstractions such as `IInboxMessage`.
5. The integration event bus persists the integration event into the transactional outbox.
6. The outbox processor later reads the persisted integration event and publishes it through the configured in-process or external messaging mechanism for cross-module delivery.

Example structure:

    ProjectCreatedDomainEvent
        ↓
    ProjectCreatedDomainEventHandler
        ↓
    ProjectCreatedIntegrationEvent

Example handler:

    internal sealed class ProjectCreatedDomainEventHandler
        : INotificationHandler<ProjectCreatedDomainEvent>
    {
        private readonly IIntegrationEventBus _integrationEventBus;

        public ProjectCreatedDomainEventHandler(IIntegrationEventBus integrationEventBus)
        {
            _integrationEventBus = integrationEventBus;
        }

        public async Task HandleAsync(
            ProjectCreatedDomainEvent notification,
            CancellationToken cancellationToken = default)
        {
            var integrationEvent = new ProjectCreatedIntegrationEvent(
                Guid.NewGuid(),
                notification.ProjectId.Value,
                notification.Name);

            await _integrationEventBus.PublishAsync(
                integrationEvent,
                cancellationToken);
        }
    }

Rules:

- Domain events MUST remain internal to their originating module.
- Domain events MUST NOT be consumed directly by other modules.
- Mapping from domain events to integration events MUST occur in the Application layer.
- Integration events MUST be published through `IIntegrationEventBus`.
- For cross-module communication, the transactional outbox MUST persist integration events rather than internal domain events.
- Integration events SHOULD live in the module Contracts project.
- Integration events MUST represent stable cross-module contracts.

## Consequences

Positive:

- Clear separation between internal domain behavior and external integration contracts.
- Preserves module encapsulation by preventing cross-module consumption of domain events.
- Standardizes the bridge pattern in the Application layer.
- Works naturally with the transactional outbox.
- Aligns with ADR-0039 by keeping producer-side publishing independent from Inbox abstractions.
- Enables reliable and eventually consistent module communication.

Negative:

- Requires explicit mapping code for each exported domain event.
- Introduces an extra application-layer handler per integration boundary.
- Requires discipline to keep integration contracts stable and versioned independently from domain models.
