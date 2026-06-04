# ADR 0005: Introduce Domain Events

## Status

Accepted

## Context

In Domain-Driven Design, important state changes within aggregates often represent meaningful events in the domain. These events may need to trigger additional behaviors such as updating other aggregates, initiating workflows, or integrating with other parts of the system.

If such behaviors are implemented directly inside aggregates or services, it can lead to tight coupling and reduced flexibility. The domain model should remain focused on expressing business rules while allowing other parts of the system to react to domain changes.

A mechanism is needed to capture and expose significant domain events without tightly coupling domain logic to infrastructure or application concerns.

## Decision

We will introduce Domain Events as a first-class concept in the domain layer.

Domain Events will represent significant business occurrences that happen within aggregates.

The design will include:

- A marker interface for domain events
- A base class for common event behavior
- Support for aggregates to collect domain events during state changes

Aggregates will record domain events when important domain actions occur. These events can later be dispatched by the application layer or infrastructure.

Example conceptual usage:

    OrderPlacedEvent
    CustomerRegisteredEvent
    PaymentCompletedEvent

## Consequences

Positive:

- Clear representation of important domain occurrences
- Reduced coupling between aggregates and side effects
- Improved extensibility of the system
- Better alignment with DDD tactical patterns

Negative:

- Additional infrastructure is required to dispatch events
- Increased architectural complexity
- Developers must understand event-driven modeling concepts
