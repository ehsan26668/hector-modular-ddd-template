# ADR 0016: Integrate Domain Event Dispatching with EF Core Save Pipeline

## Status

Accepted

## Context

The project already defines domain events in the Domain layer and stores them inside aggregate roots.

ADR-0013 defined the persistence strategy for domain event dispatching. According to that decision, domain events must be collected from tracked aggregates during the persistence workflow, state changes must be persisted first, and domain events must be dispatched only after a successful database save operation.

ADR-0015 introduced a concrete mediator-based implementation of `IDomainEventDispatcher` in the Persistence layer. This dispatcher publishes domain events through the internal application mediator.

The remaining architectural gap is the integration point between EF Core and the domain event dispatcher.

`HectorDbContext` is the framework-controlled base persistence component. It must coordinate the save pipeline so that domain events raised by aggregates are automatically collected, dispatched, and cleared without requiring application services to manually publish them.

This decision defines how domain event dispatching is integrated into the EF Core save pipeline.

## Decision

Domain event dispatching will be integrated directly into `HectorDbContext` by overriding `SaveChangesAsync`.

The save pipeline will follow this order:

1. Collect domain events from tracked aggregate roots.
2. Persist state changes by calling the EF Core base `SaveChangesAsync`.
3. Dispatch the collected domain events through `IDomainEventDispatcher`.
4. Clear domain events from the aggregate roots after successful dispatch.

Expected flow:

    SaveChangesAsync(cancellationToken)
    Collect domain events from tracked aggregates
    Persist changes to database
    Dispatch collected domain events through IDomainEventDispatcher
    Clear domain events from aggregates

The dispatch operation must happen only after the database save operation has completed successfully.

If persistence fails, domain events must not be dispatched and must remain available on the aggregate roots.

If dispatch fails after a successful database save, the exception will be allowed to propagate to the caller. In this case, the database transaction has already been completed by EF Core unless the caller explicitly controls a transaction. This behavior is accepted for the current in-process dispatching model and will be revisited if an outbox-based integration-event pipeline is introduced.

`HectorDbContext` will depend on `IDomainEventDispatcher` through constructor injection.

The implementation will inspect EF Core tracked entries and select aggregate roots that contain domain events.

The implementation will not contain domain logic. It only coordinates persistence and dispatching infrastructure.

The scope of this decision is limited to in-process domain event dispatching inside the EF Core save pipeline. It does not introduce an outbox pattern, retries, distributed transactions, background processing, or integration-event publication.

## Implementation Notes

The base DbContext shape is expected to be similar to:

    public abstract class HectorDbContext : DbContext
    {
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        protected HectorDbContext(
            DbContextOptions options,
            IDomainEventDispatcher domainEventDispatcher)
            : base(options)
        {
            _domainEventDispatcher = domainEventDispatcher;
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            var aggregatesWithDomainEvents = ChangeTracker
                .Entries<AggregateRoot>()
                .Select(entry => entry.Entity)
                .Where(aggregateRoot => aggregateRoot.DomainEvents.Any())
                .ToList();

            var domainEvents = aggregatesWithDomainEvents
                .SelectMany(aggregateRoot => aggregateRoot.DomainEvents)
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

            foreach (var aggregateRoot in aggregatesWithDomainEvents)
            {
                aggregateRoot.ClearDomainEvents();
            }

            return result;
        }
    }

Exact implementation details may differ depending on the final `AggregateRoot` API and EF Core mapping constraints.

## Testing Strategy

This decision will be validated through integration tests in the Persistence integration test project.

The tests must verify that:

- domain events raised by tracked aggregate roots are dispatched when `SaveChangesAsync` succeeds
- all collected domain events are passed to `IDomainEventDispatcher`
- the provided `CancellationToken` is passed through to the dispatcher
- domain events are cleared from aggregate roots after successful dispatch
- domain events are not dispatched if `base.SaveChangesAsync` fails
- domain events remain on aggregate roots if persistence fails
- dispatch exceptions are propagated to the caller

Unit tests may be added for helper methods if the collection logic becomes non-trivial, but the main behavior belongs to integration tests because it depends on EF Core tracking behavior.

## Consequences

Positive:

- Automates domain event dispatching during persistence
- Keeps application services free from manual domain event publishing
- Preserves the ADR-0013 rule that dispatch happens only after successful persistence
- Reuses the mediator-based dispatcher introduced in ADR-0015
- Centralizes domain event dispatch coordination inside the persistence infrastructure
- Improves consistency across modules using the shared `HectorDbContext`

Negative:

- `HectorDbContext` gains a dependency on `IDomainEventDispatcher`
- Dispatch failures can occur after successful database persistence
- In-process dispatching does not provide cross-process reliability guarantees
- No retry, outbox, or durable messaging behavior is provided at this stage
- Save pipeline behavior becomes more opinionated and framework-controlled

## Alternatives Considered

### Use EF Core SaveChanges Interceptors

EF Core interceptors could be used to hook into the save pipeline and dispatch domain events.

This option was rejected for now because overriding `SaveChangesAsync` is simpler, more explicit, easier to test, and provides direct control over ordering.

Interceptors may be reconsidered later if cross-cutting persistence behaviors become more complex.

### Dispatch Domain Events Before Persistence

Domain events could be dispatched before calling `base.SaveChangesAsync`.

This option was rejected because handlers could observe or act on state that has not been successfully persisted. It would also violate the persistence strategy defined in ADR-0013.

### Require Application Services to Dispatch Events Manually

Application services could collect and dispatch domain events after saving changes.

This option was rejected because it leaks infrastructure workflow concerns into application use cases and makes consistency dependent on developer discipline.

### Use Outbox Pattern Immediately

The outbox pattern would provide stronger reliability guarantees for integration events and cross-process messaging.

This option was rejected for the current step because the present requirement is limited to in-process domain event dispatching. Outbox support may be introduced later as a separate architectural decision.

## Related Decisions

- [ADR-0013: Base DbContext and Domain Event Dispatch Strategy](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md)
- [ADR-0014: Internal Mediator for Application Messaging](/docs/adr/0014-adopt-internal-mediator-for-CQRS.md)
- [ADR-0015: Implement Mediator-Based Domain Event Dispatcher in Persistence](/docs/adr/0015-implement-mediator-based-domain-event-dispatcher.md)
