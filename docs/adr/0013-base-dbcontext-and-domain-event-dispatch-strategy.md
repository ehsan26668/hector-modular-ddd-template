# ADR 0013: Base DbContext and Domain Event Dispatch Strategy

## Status

Accepted — Domain event dispatching strategy superseded by ADR-0021

## Context

The project follows a modular monolith architecture with Domain-Driven Design tactical patterns implemented in the Building Blocks layer.

Domain entities derive from `AggregateRoot` and can produce domain events. These events must be handled reliably when changes are persisted.

Persistence capabilities already exist through EF Core conventions defined in earlier decisions, including automatic mapping for strongly typed identifiers.

At the time this ADR was originally introduced, the goal was to standardize:

- aggregate persistence across modules
- transaction coordination
- domain event collection during persistence
- consistent persistence behavior across all module DbContexts

A shared persistence base was needed to avoid duplicated infrastructure code and inconsistent save-pipeline behavior across modules.

This ADR introduced the shared `HectorDbContext` abstraction for that purpose.

However, the original form of this ADR assumed that collected domain events would be dispatched directly from the EF Core save pipeline through an in-memory dispatcher.

As the architecture matured, that assumption proved insufficient for production-grade reliability. Direct dispatch from the save pipeline does not provide the same failure-handling, durability, and operational guarantees as a transactional persistence strategy.

ADR-0021 later introduced the Transactional Outbox pattern as the governing strategy for reliable event delivery.

Therefore:

- this ADR remains authoritative for introducing the shared `HectorDbContext`
- this ADR remains authoritative for standardizing save-pipeline behavior and persistence conventions
- the original direct domain event dispatching strategy described here is no longer the governing implementation

## Decision

We introduce a shared base class named `HectorDbContext` in the `Hector.BuildingBlocks.Persistence` project.

This base class extends `DbContext` and provides standardized infrastructure behavior for all module DbContexts.

All module-specific DbContexts must inherit from `HectorDbContext`.

Responsibilities of the base context:

1. Provide a consistent Unit of Work implementation using EF Core `DbContext`.
2. Automatically collect domain events from tracked aggregates during persistence.
3. Apply shared persistence conventions defined in the Building Blocks layer, including strongly typed ID mapping.
4. Persist domain events transactionally as outbox messages according to ADR-0021.
5. Clear domain events from aggregates only after successful persistence.

The save pipeline is conceptually defined as follows:

    SaveChangesAsync()
        ↓
    Collect domain events from tracked aggregates
        ↓
    Convert domain events to OutboxMessage records
        ↓
    Persist aggregate changes and outbox messages in the same transaction
        ↓
    Clear domain events from aggregates after successful persistence
        ↓
    Publish events asynchronously through the outbox processing pipeline

This ADR governs the existence and responsibilities of the shared base DbContext.

The reliable publication of persisted outbox messages is governed by ADR-0021 and the ADRs that follow it.

The following infrastructure abstractions are involved in this strategy:

    HectorDbContext
    OutboxMessage
    IOutboxEventSerializer
    IStronglyTypedIdAssemblyProvider

`IDomainEventDispatcher` may still exist elsewhere in the architecture, but it is not the governing mechanism for `HectorDbContext` persistence behavior.

Example conceptual usage:

    public class SalesDbContext : HectorDbContext
    {
        public SalesDbContext(
            DbContextOptions<SalesDbContext> options,
            IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider,
            IOutboxEventSerializer outboxSerializer)
            : base(options, stronglyTypedIdAssemblyProvider, outboxSerializer)
        {
        }
    }

Aggregates continue to raise domain events through the `AggregateRoot` base class.

Persistence infrastructure is responsible for collecting those events and storing them as outbox messages within the same database transaction as aggregate state changes.

## Consequences

Positive:

- Provides a single, consistent persistence strategy across all modules.
- Preserves a shared EF Core base context for all module DbContexts.
- Centralizes Unit of Work behavior in the Building Blocks layer.
- Ensures domain events are collected automatically during persistence.
- Enables transactional persistence of aggregate changes and outbox messages.
- Supports production-ready reliability through the Transactional Outbox pattern.
- Reduces infrastructure duplication across module DbContexts.
- Keeps domain logic free from EF Core dependencies.

Negative:

- Introduces additional infrastructure complexity in the shared persistence layer.
- Requires module DbContexts to inherit from the shared base class.
- Defers actual event publication to asynchronous background processing rather than immediate in-memory dispatch.
- Requires serializer and outbox-related infrastructure to be available in persistence.

## Related Decisions

- ADR-0012: Automated Persistence Mapping for Strongly Typed IDs
- ADR-0021: Adopt Transactional Outbox for Domain Events
- ADR-0022 and later: Outbox processing, cleanup, reliability, and delivery concerns
