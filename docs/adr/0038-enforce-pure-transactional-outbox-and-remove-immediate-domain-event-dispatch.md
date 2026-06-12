# ADR-0038: Enforce Pure Transactional Outbox and Remove Immediate Domain Event Dispatch

## Status

Accepted

## Context

ADR-0021 adopted the Transactional Outbox pattern for reliable domain event publication.
However, the current implementation of `HectorDbContext.SaveChangesAsync` still performs
immediate in-process domain event dispatch through `IDomainEventDispatcher` after persistence.

This results in a hybrid publication model:

1. Domain events are persisted to the outbox.
2. The same domain events are also dispatched immediately in-process.

This hybrid behavior violates the intent of ADR-0021 and introduces the following risks:

- duplicate event handling
- inconsistent runtime behavior between request thread and background processor
- coupling between persistence and in-process dispatch
- misleading test expectations that assume synchronous side effects

## Decision

When Transactional Outbox is enabled, `HectorDbContext` must not dispatch domain events directly.

The save pipeline must behave as follows:

1. collect domain events from tracked aggregates
2. serialize and store them as outbox messages in the same EF Core save operation
3. commit the transaction
4. clear domain events only after a successful save
5. rely exclusively on the outbox processor for event publication

`IDomainEventDispatcher` remains available as a building block, but it is no longer part of
the `HectorDbContext` save pipeline.

## Consequences

### Positive

- aligns runtime behavior with ADR-0021
- eliminates duplicate domain event publication
- makes publication durability explicit
- simplifies persistence responsibilities
- improves test clarity by separating persistence concerns from asynchronous publication concerns

### Negative

- existing tests that assume synchronous domain event side effects must be rewritten
- modules relying on immediate event handler execution must now validate outbox persistence first
- end-to-end behavior requires background processor execution in tests where publication is expected

## Related ADRs

- ADR-0016: Integrate Domain Event Dispatching with EF Core Save Pipeline
- ADR-0021: Adopt Transactional Outbox for Domain Events
- ADR-0022: Outbox Background Processor

## Notes

ADR-0016 introduced immediate in-process domain event dispatch as part of the EF Core save pipeline.

ADR-0038 supersedes that behavior when the Transactional Outbox pattern is enabled,
making the outbox the single publication mechanism for domain events.
