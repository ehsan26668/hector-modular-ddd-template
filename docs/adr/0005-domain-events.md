# ADR 0005: Domain Events Handling

## Status

Accepted

## Context

In a Domain-Driven Design architecture, important changes in the domain should produce Domain Events.  
These events allow other parts of the system to react without tight coupling.

We need a mechanism to collect domain events inside aggregates and dispatch them after persistence.

## Decision

- Introduce a marker interface `IDomainEvent`.
- `AggregateRoot<TId>` will inherit from `Entity<TId>`.
- Aggregate roots will maintain an internal collection of domain events.
- Events can be raised using `RaiseDomainEvent`.
- Events can be retrieved and cleared after successful persistence.

## Consequences

Positive:

- Enables decoupled domain communication.
- Supports eventual consistency between modules.
- Aligns with common DDD practices.

Negative:

- Requires infrastructure logic to dispatch events after SaveChanges.
