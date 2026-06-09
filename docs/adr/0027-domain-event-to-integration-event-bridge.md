# ADR 0027: Domain Event to Integration Event Bridge

## Status

Proposed

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

However, modules also need to communicate with other modules and external systems.

For that purpose, the system defines **Integration Events**, such as:

- ProjectCreatedIntegrationEvent

Integration events represent stable contracts intended for cross-module communication or external messaging infrastructure (e.g., message brokers).

Currently, there is no standardized mechanism that converts domain events into integration events. Without a clear pattern, each module may implement its own mapping logic, leading to:

- duplication
- inconsistent integration boundaries
- tight coupling between modules
- lack of a clear architectural rule

To maintain a clean modular architecture, the system needs a clear separation between:

- internal domain events
- externally visible integration events

and a consistent mechanism to transform one into the other.

## Decision

The system will introduce a **Domain Event → Integration Event Bridge**.

Domain events remain internal to their module. Integration events represent external communication contracts.

The bridge is implemented using **domain event handlers** that map domain events to integration events.

The flow is:

1. A domain event is raised inside an aggregate.
2. The domain event is stored in the transactional outbox.
3. The outbox processor publishes the domain event.
4. A domain event handler maps the domain event to an integration event.
5. The integration event is published through the mediator and may later be delivered to other modules or message brokers.

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
        private readonly IMediator _mediator;

        public ProjectCreatedDomainEventHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(
            ProjectCreatedDomainEvent notification,
            CancellationToken cancellationToken)
        {
            var integrationEvent = new ProjectCreatedIntegrationEvent(
                notification.ProjectId,
                notification.Name);

            await _mediator.PublishAsync(integrationEvent, cancellationToken);
        }
    }

Rules:

- Domain events MUST NOT be consumed directly by other modules.
- Integration events MUST represent stable contracts.
- Mapping from domain events to integration events MUST occur in the Application layer.
- Integration events SHOULD live in the Contracts project of the module.

This approach keeps domain logic isolated while enabling controlled communication across module boundaries.

## Consequences

Positive:

- Clear separation between internal domain behavior and external communication.
- Integration contracts remain stable and independent from domain model evolution.
- Modules communicate through explicit integration events.
- Compatible with the existing transactional outbox implementation.
- Enables future integration with message brokers such as Kafka or RabbitMQ.

Negative:

- Additional mapping code between domain events and integration events.
- Slight increase in architectural complexity.
- Developers must maintain both domain and integration event models.
