# ADR 0024: Adopt Distributed Locking for Outbox Processor

## Status

Implemented

Implemented on: Date: 2026-06-13

## Context

ADR-0021 introduced the Transactional Outbox pattern to ensure domain events are persisted atomically with business data changes. ADR-0022 introduced a background processor responsible for polling and publishing pending outbox messages.

In a production environment, multiple application instances may run concurrently. If each instance independently polls the same outbox table without coordination, the same message may be claimed and published more than once at the same time. This creates a race condition in the outbox processing pipeline and weakens delivery guarantees.

Because the system is designed to scale horizontally, the outbox processor must coordinate ownership of pending messages across competing nodes without relying on in-memory synchronization primitives or single-instance assumptions.

The solution must align with the existing architecture:

- Modular Monolith with one `DbContext` per feature module
- EF Core as the persistence technology
- Transactional Outbox already persisted in the same database
- At-least-once message delivery semantics
- Inbox-based idempotent handling already introduced by ADR-0023

## Decision

We will adopt a database-backed distributed locking strategy for the outbox processor using a lease-based ownership model.

Each outbox message may be temporarily claimed by a processor instance using:

- `LockId`: a unique identifier representing the current processing owner
- `LockedUntil`: a UTC timestamp representing the lease expiration

The processor workflow is:

1. Select a batch of eligible outbox message identifiers where:
   - `ProcessedOn` is `null`
   - `RetryCount` is below the configured retry limit
   - `LockedUntil` is either `null` or expired

2. Attempt to atomically claim those messages through a conditional database update that sets:
   - `LockId`
   - `LockedUntil`

3. Load only the messages successfully claimed by the current processor instance using the generated `LockId`

4. Publish the claimed messages in occurrence order

5. On success:
   - set `ProcessedOn`
   - set `LastAttemptedOn`
   - clear `Error`
   - release the lock by clearing `LockId` and `LockedUntil`

6. On failure:
   - increment `RetryCount`
   - set `LastAttemptedOn`
   - persist the error message
   - release the lock by clearing `LockId` and `LockedUntil`

Lock duration, batch size, retry policy, retention period, and cleanup batch size are configured through `OutboxOptions`.

This locking strategy is implemented at the persistence layer and uses the database as the coordination point between concurrent processor instances.

## Consequences

### Positive

- Prevents multiple processor instances from simultaneously owning the same outbox message
- Supports horizontal scaling of background processing
- Keeps coordination logic within the same persistence boundary as the outbox table
- Avoids dependence on external distributed lock infrastructure
- Aligns well with EF Core and the current building blocks architecture
- Preserves ordering within a claimed batch
- Works naturally with the Inbox pattern introduced in ADR-0023

### Trade-offs

- The locking model is lease-based, not permanent ownership
- If processing takes longer than the configured lock duration, another processor may reclaim the same message after lock expiration
- If publishing fails partway through a batch, previously published messages in that batch may be retried
- Delivery semantics remain at-least-once, not exactly-once
- Correctness depends on choosing a lock duration that is appropriate for expected processing latency

### Operational Notes

- `LockDuration` must be configured conservatively relative to batch size and expected publish latency
- Consumers must remain idempotent
- Retry exhaustion is handled through retry count limits; poison-message handling may evolve in a later ADR
- Cleanup policies remain governed separately by ADR-0025

## Alternatives Considered

### 1. No distributed locking

Each processor instance would poll and publish pending messages independently with no ownership coordination.

Rejected because concurrent instances could process the same message at the same time, producing unnecessary duplicates and weakening system behavior under scale-out.

### 2. Single application instance only

Restrict outbox processing to a single deployed node.

Rejected because it introduces a single point of failure and prevents horizontal scalability.

### 3. External distributed lock provider

Use Redis, ZooKeeper, or another external coordinator for distributed locking.

Rejected for now because it adds infrastructure complexity and operational overhead without sufficient justification for the current architecture. The outbox table itself already provides a suitable coordination boundary.

### 4. Database-specific locking primitives

Use vendor-specific SQL features such as row locking hints, `SKIP LOCKED`, or equivalent mechanisms.

Rejected for now because the current implementation favors an EF Core-based, provider-agnostic approach that remains simple and portable within the template.

## Related ADRs

- ADR-0021: Adopt Transactional Outbox for Domain Event Publishing
- ADR-0022: Implement Outbox Background Processor
- ADR-0023: Adopt Inbox Pattern for Idempotent Event Handling
- ADR-0025: Outbox Cleanup and Retention Policy
- ADR-0033: Event Ordering and Delivery Guarantees
- ADR-0034: Dead-letter and Poison Message Handling
- ADR-0035: Consumer Idempotency Strategy
