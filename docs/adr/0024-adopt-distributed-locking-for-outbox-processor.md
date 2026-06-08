# ADR 0024: Adopt Distributed Locking for Outbox Processor

## Status

Accepted

## Context

ADR‑0021 introduced the Transactional Outbox pattern to guarantee reliable persistence of domain events.

ADR‑0022 introduced a background Outbox Processor responsible for polling the OutboxMessages table and publishing events.

In single‑instance deployments, a single processor instance reads and processes messages safely.

However, in real production environments applications are typically deployed with multiple instances for scalability and high availability. In such scenarios, multiple Outbox Processor instances may run concurrently.

Without coordination between instances, several issues may occur:

- multiple processors may pick the same outbox message
- the same domain event may be published multiple times
- database contention may increase
- message ordering may become inconsistent

Although idempotent consumers (ADR‑0023) protect downstream systems, preventing duplicate publishing at the source is still desirable to reduce unnecessary load and improve system efficiency.

To safely support horizontal scaling of the Outbox Processor, a distributed coordination mechanism is required.

## Decision

We will introduce a distributed locking strategy for the Outbox Processor to coordinate concurrent processors across multiple application instances.

The Outbox Processor will acquire ownership of messages before processing them by using a lease‑based locking mechanism stored in the database.

Each message will include lock metadata:

`LockId`
`LockedUntil`

The processing workflow will follow these steps:

1. A processor instance generates a unique LockId.
2. The processor selects a batch of unprocessed messages where:
    - ProcessedOn is null
    - LockedUntil is null or expired
3. The processor updates the selected rows by setting:
    - LockId
    - LockedUntil = current time + lock duration
4. Only the processor that owns the lock processes the messages.
5. After successful processing, the processor sets:
    - ProcessedOn
    - clears the lock metadata.

If a processor crashes or becomes unavailable, the lock eventually expires and another processor can safely pick up the message.
The locking mechanism will rely solely on the database to avoid introducing additional infrastructure dependencies.

## Consequences

Positive:

- enables safe horizontal scaling of the Outbox Processor
- prevents multiple processors from publishing the same event concurrently
- avoids duplicate message publishing at the source
- improves system stability in multi‑instance deployments
- requires no external distributed lock service

Negative:

- introduces additional columns and update operations in the Outbox table
- requires careful tuning of lock duration
- slight increase in database load during polling
- messages may be temporarily locked if a processor crashes before completing processing
