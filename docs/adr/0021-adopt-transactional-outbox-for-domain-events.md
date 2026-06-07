# ADR 0021: Adopt Transactional Outbox for Domain Event Publishing

## Status

Accepted

## Context

The current persistence pipeline in `HectorDbContext.SaveChangesAsync` persists aggregates first and then dispatches domain events immediately through `IDomainEventDispatcher` backed by `IMediator`.

This design is simple and works for in-process dispatch, but it has a reliability gap:

- if the database commit succeeds and event dispatch fails, the aggregate changes are persisted while the event is not reliably published
- there is no durable retry mechanism for failed event publication
- event delivery is coupled to the request thread and the application process lifetime

We need a more reliable mechanism for publishing domain events while preserving the existing modular DDD structure and EF Core save pipeline conventions.

## Decision

We will adopt the Transactional Outbox pattern for domain event publication.

The new persistence flow will be:

1. collect domain events from tracked aggregates
2. serialize them into outbox records
3. persist aggregates and outbox records in the same database transaction
4. clear domain events from aggregates only after successful save
5. publish outbox messages asynchronously via a background dispatcher
6. mark outbox messages as processed after successful publication

The outbox mechanism will live in the shared persistence building blocks so that it can be reused by all feature modules.

## Consequences

Positive:

- domain event publication becomes durable and transactionally consistent with aggregate persistence
- failed publication can be retried safely
- event handling is decoupled from the request lifecycle
- the architecture becomes more suitable for future integration with external message brokers

Negative:

- the system gains additional infrastructure complexity
- serialization and deserialization of event payloads must be maintained carefully
- a background worker is required to process pending outbox messages
- duplicate publication is still possible under retry scenarios, so consumers must remain idempotent
