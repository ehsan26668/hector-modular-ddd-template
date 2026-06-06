# ADR 0015: Implement Mediator-Based Domain Event Dispatcher in Persistence

## Status

Accepted

## Context

The project already defines domain events in the Domain layer and establishes the persistence dispatch strategy in ADR-0013.

Aggregates derive from `AggregateRoot` and store raised domain events internally. The `HectorDbContext` save pipeline is intended to collect these domain events from tracked aggregates, persist state changes successfully, dispatch the collected events through `IDomainEventDispatcher`, and then clear the domain event buffers.

At the same time, the internal mediator introduced by ADR-0014 is now available as the framework-controlled mechanism for in-process message dispatching. It already supports asynchronous execution, dependency injection, and multi-handler notification publishing.

A concrete implementation of `IDomainEventDispatcher` is now required so that persistence infrastructure can publish domain events through the internal mediator without introducing any dependency on external mediator libraries.

This decision is needed to connect the Domain event model to the Application messaging pipeline while preserving the architectural boundaries defined in ADR-0013 and ADR-0014.

## Decision

A mediator-based implementation of `IDomainEventDispatcher` will be introduced in `Hector.BuildingBlocks.Persistence`.

The implementation will use the internal `IMediator` abstraction from `Hector.BuildingBlocks.Application` to publish domain events as in-process notifications.

The dispatcher will:

- accept a sequence of `IDomainEvent`
- publish each domain event asynchronously through the mediator
- pass through the provided `CancellationToken`
- remain infrastructure-focused and contain no domain logic

The persistence save pipeline will use this dispatcher after a successful database save operation, in accordance with ADR-0013.

To enable this integration, domain events will be treated as mediator notifications.

The scope of this decision is limited to in-process dispatching of domain events from persistence to application handlers. It does not introduce integration-event publishing, outbox processing, retries, or cross-process messaging.

Expected interaction:

    SaveChangesAsync()
    Collect domain events from tracked aggregates
    Persist changes to database
    Dispatch domain events through IDomainEventDispatcher
    Clear domain events from aggregates

Dispatcher shape:

    public sealed class DomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default);
    }

Mediator integration rule:

    each IDomainEvent will be published through IMediator.PublishAsync(...)

Registration scope:

    IDomainEventDispatcher will be registered in the dependency injection container
    and consumed by HectorDbContext or its save pipeline implementation

## Consequences

Positive:

- Completes the bridge between Domain Events and the internal mediator
- Reuses the existing framework-controlled messaging infrastructure
- Keeps the Domain layer free from application and infrastructure concerns
- Preserves the ADR-0013 rule that dispatch happens after successful persistence
- Enables multiple in-process handlers for a single domain event

Negative:

- Adds a dependency from Persistence to Application messaging abstractions
- Domain event dispatch remains in-process only
- Failures during post-save dispatch must be handled carefully by the persistence workflow
- Event ordering guarantees depend on the mediator notification publishing behavior and save pipeline design
