# ADR-0039: Separate Integration Event from Inbox Message

## Status

Implemented

## Context

In our current messaging infrastructure, the `IIntegrationEventBus.PublishAsync` method accepts an `IInboxMessage` interface.
While this works for the Outbox pattern (where events are stored before publishing), it creates a "Leak of Abstraction".
The concept of an "Inbox" belongs to the consumer side, while "Publishing" belongs to the producer side.
Mixing these two interfaces makes the domain models dependent on infrastructure concerns they shouldn't care about.

## Decision

We will decouple these concerns by introducing a dedicated marker interface for integration events:

1. Introduce a dedicated marker interface `IIntegrationEvent` that extends `INotification` from the Application layer.
2. Update `IIntegrationEventBus.PublishAsync` to accept `IIntegrationEvent` instead of `IInboxMessage`.
3. The producer side will depend only on `IIntegrationEvent`.
4. Inbox-related abstractions remain consumer-side concerns.

## Consequences

- **Positive:** Clear separation between producer and consumer concerns.
- **Positive:** Prevents infrastructure leakage from Persistence into Application layer.
- **Positive:** Cleaner and more intention-revealing Event Bus API.
- **Neutral:** Requires refactoring existing integration events.

## Status Update (Final)

The separation between Integration Events and Inbox Messages is now fully implemented.
The publish path depends exclusively on IIntegrationEvent and IIntegrationEventBus,
and Inbox concerns remain strictly consumer-side. Tests for ProjectCreatedDomainEventHandler
and OutboxIntegrationEventBus verify and enforce the architecture boundaries.
