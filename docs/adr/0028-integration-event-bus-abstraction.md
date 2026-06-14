# ADR 0028: Integration Event Bus Abstraction

## Status

Superseded by ADR-0027 and ADR-0039

> ⚠️ This ADR has been superseded by:
>
> - [ADR-0027: Domain Event to Integration Event Bridge](/docs/adr/0027-domain-event-to-integration-event-bridge.md)
> - [ADR-0039: Separate Integration Event from Inbox Message](/docs/adr/0039-separate-integration-event-from-inbox-message.md)
>
> The abstraction and responsibilities described here were refined and formally adopted
> in the later ADRs. This document remains for historical context only.

## Context

The system already supports domain events and integration events.

The current event flow is:

1. Aggregates raise domain events.
2. Domain events are stored in the transactional outbox.
3. The outbox processor publishes the events through the internal mediator.
4. Domain event handlers may convert them into integration events.

Integration events represent cross-module or external communication contracts.

In the current architecture, integration events are still published through the internal mediator. While this works inside a modular monolith, it does not define a clear abstraction for delivering integration events to external systems such as message brokers.

Future system evolution may require publishing integration events to infrastructure such as:

- RabbitMQ
- Kafka
- Azure Service Bus
- other messaging platforms

Without a dedicated abstraction, application code could become tightly coupled to a specific messaging technology, making it difficult to change infrastructure later.

To preserve clean architecture boundaries and maintain flexibility, the system needs a dedicated abstraction for publishing integration events.

## Decision

The system will introduce an **Integration Event Bus abstraction**.

Application code will publish integration events through an interface instead of directly interacting with messaging infrastructure.

Example interface:

    public interface IIntegrationEventBus
    {
        Task PublishAsync(object integrationEvent, CancellationToken cancellationToken = default);
    }

Application services and domain event handlers will depend on this abstraction.

Example usage:

    await _integrationEventBus.PublishAsync(
        new ProjectCreatedIntegrationEvent(projectId, name),
        cancellationToken);

Infrastructure modules will provide implementations of this interface.

Possible implementations include:

- InMemoryIntegrationEventBus (for modular monolith communication)
- RabbitMqIntegrationEventBus
- KafkaIntegrationEventBus
- AzureServiceBusIntegrationEventBus

The default implementation for the modular monolith will publish events internally using the mediator.

Example:

    internal sealed class InMemoryIntegrationEventBus : IIntegrationEventBus
    {
        private readonly IMediator _mediator;

        public InMemoryIntegrationEventBus(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task PublishAsync(object integrationEvent, CancellationToken cancellationToken = default)
        {
            return _mediator.PublishAsync((INotification)integrationEvent, cancellationToken);
        }
    }

Rules:

- Application layer MUST depend only on the abstraction.
- Messaging infrastructure MUST be implemented in the Infrastructure layer.
- Integration events MUST remain simple data contracts.
- Domain layer MUST NOT reference the event bus.

This design keeps the application independent of messaging infrastructure while enabling future distributed messaging capabilities.

## Consequences

Positive:

- Decouples application logic from messaging infrastructure.
- Enables easy integration with external message brokers.
- Improves testability through mocking of the event bus.
- Supports future transition from modular monolith to microservices.
- Keeps architecture aligned with clean architecture principles.

Negative:

- Introduces an additional abstraction layer.
- Requires infrastructure implementations for each messaging technology.
- Developers must understand the distinction between mediator events and integration events.
