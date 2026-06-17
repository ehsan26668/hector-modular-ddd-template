# ADR-0038: Enforce Transactional Outbox for Integration Events

## Status

Accepted

## Context

ADR-0021 adopted the Transactional Outbox pattern to make event publication reliable.
Later ADRs refined the event model by separating internal domain events from external
integration events:

- domain events represent facts that happened inside a bounded context
- integration events represent stable contracts published outside a bounded context
- ADR-0027 introduced the Domain Event to Integration Event Bridge
- ADR-0028 introduced the Integration Event Bus abstraction
- ADR-0040 requires module-level registration of outbox event contracts

The previous interpretation of ADR-0038 assumed that domain events should be serialized
and stored directly in the outbox. That interpretation conflicts with the current
architecture because domain events are internal domain concepts, while outbox messages are
durable integration publication records.

Persisting domain events directly to the outbox would introduce the following risks:

- leaking internal domain model details into external messaging contracts
- coupling the domain layer to serialization, versioning, and contract stability concerns
- bypassing the Domain Event to Integration Event Bridge
- making Contracts assemblies and module-level contract registration ambiguous
- forcing outbox infrastructure to resolve and publish internal domain event types
- reducing the ability to evolve the domain model independently from integration contracts

At the same time, immediate publication of integration events from the request thread must
be avoided because it can create consistency gaps between database state changes and
message broker publication.

## Decision

Transactional Outbox is the only supported mechanism for publishing integration events.

`HectorDbContext` must not serialize or persist domain events directly as outbox messages.

Domain events remain internal in-process notifications. They may be dispatched inside the
save pipeline so application-level handlers can react to domain facts and create
integration events when needed.

Integration events are the only events that may be persisted as outbox messages.

The save pipeline must behave as follows:

1. collect domain events from tracked aggregates
2. dispatch domain events in-process before committing changes
3. allow domain event handlers to create integration events through `IIntegrationEventBus`
4. persist integration events as outbox messages in the same EF Core save operation
5. commit aggregate state changes and outbox messages atomically
6. clear domain events only after a successful save
7. rely exclusively on the outbox processor for publishing integration events

`IDomainEventDispatcher` remains part of the `HectorDbContext` save pipeline, but its
responsibility is limited to in-process domain event handling.

`IIntegrationEventBus` must not publish directly to an external broker in the request
thread. Its persistence implementation must store integration events in the outbox.

The outbox processor is the only component responsible for publishing integration events
to external transports.

## Architectural Rules

- Domain events must not be treated as external messaging contracts.
- Domain events must not require outbox metadata, event names, contract versions, or transport-specific attributes.
- Integration events must be explicit contracts owned by module Contracts assemblies.
- Integration events must be registered using module-level contract registration.
- Application handlers are responsible for translating domain events into integration events when cross-module or external notification is required.
- Infrastructure is responsible for serializing integration events and storing them as outbox messages.
- External message publication must happen asynchronously through the outbox processor.

## Consequences

### Positive

- preserves domain model purity
- aligns the outbox with integration event contracts
- keeps ADR-0027 Domain Event to Integration Event Bridge meaningful
- keeps ADR-0040 module-level outbox contract registration meaningful
- prevents immediate external publication from the request thread
- avoids leaking internal domain event shapes to external consumers
- allows domain events and integration events to evolve independently
- keeps aggregate changes and integration publication intent atomic
- clarifies the responsibilities of domain, application, persistence, and messaging layers

### Negative

- domain event handlers still execute synchronously inside the save pipeline
- handlers must avoid external side effects
- handler failures can fail the save operation
- tests must distinguish between in-process domain handling and asynchronous integration publication
- modules must explicitly bridge domain events to integration events when publication is required

### Neutral

- `IDomainEventDispatcher` remains necessary for internal application behavior.
- `IIntegrationEventBus` remains necessary for durable integration publication.
- Outbox persistence validates integration event storage, not direct domain event storage.
- End-to-end publication tests require the outbox processor.

## Supersedes

This ADR refines ADR-0016.

ADR-0016 introduced domain event dispatching as part of the EF Core save pipeline.
That behavior remains valid only for in-process domain event handling.

This ADR supersedes any interpretation that domain events should be directly published
externally or directly persisted as outbox messages.

## Related ADRs

- ADR-0016: Integrate Domain Event Dispatching with EF Core Save Pipeline
- ADR-0021: Adopt Transactional Outbox
- ADR-0022: Outbox Background Processor
- ADR-0027: Domain Event to Integration Event Bridge
- ADR-0028: Integration Event Bus Abstraction
- ADR-0029: Integration Event Versioning Strategy
- ADR-0030: Event Naming and Contract Stability Rules
- ADR-0031: Event Schema Evolution Strategy
- ADR-0032: Event Metadata and Correlation Strategy
- ADR-0040: Module-Level Outbox Event Contract Registration

## Notes

The phrase "event publication" in earlier ADRs must be interpreted as integration event
publication when referring to durable outbox-based messaging.

Correct event flow:

Aggregate -> DomainEvent -> IDomainEventDispatcher -> Application Handler / Bridge -> IIntegrationEventBus -> OutboxMessage -> OutboxProcessor -> Message Broker

Incorrect event flow:

Aggregate -> DomainEvent -> OutboxMessage -> OutboxProcessor -> Message Broker

Transactional Outbox guarantees durability for integration publication intent, not direct
external publication of internal domain events.
