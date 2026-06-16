# ADR 0033: Event Ordering and Delivery Guarantees

## Status

Implemented

## Context

The system uses the Transactional Outbox pattern to persist events and a background outbox processor to publish them asynchronously.

This architecture improves reliability, but it also introduces important distributed messaging concerns:

- events may be delivered more than once
- events may be processed out of order
- retries may cause duplicate deliveries
- consumers may observe eventual consistency

As the system evolves toward more advanced event-driven workflows, it is necessary to define explicit expectations around delivery guarantees and ordering semantics.

Without a clear policy, developers may incorrectly assume stronger guarantees than the system actually provides.

## Decision

The system will adopt **at-least-once delivery** as the official event delivery guarantee.

The system will not guarantee exactly-once delivery.

Consumers must therefore be designed to be idempotent.

### Delivery Guarantee

Events stored in the outbox will be retried until they are either:

- successfully processed
- marked as failed after reaching retry limits

Because retries can occur after partial failures, the same event may be published more than once.

Therefore:

- producers guarantee durability before publish
- publishers guarantee retry
- consumers must tolerate duplicate delivery

### Ordering Guarantee

The system will preserve **best-effort ordering within a single outbox stream** using the event occurrence timestamp.

Example:

- outbox messages are selected ordered by `OccurredOn`
- batches are processed in that order when possible

However, strict global ordering is not guaranteed because:

- batching may split event sequences
- retries may delay individual messages
- multiple processor instances may run concurrently
- different modules may publish independently

Therefore, consumers must not rely on strict global ordering unless explicitly supported by future infrastructure.

### Consumer Requirements

All event consumers must be designed with the following assumptions:

- duplicate events are possible
- out-of-order delivery is possible
- eventual consistency is expected

Consumers should use the Inbox pattern or equivalent deduplication mechanisms where necessary.

### Future Infrastructure

If the system later integrates with a broker such as Kafka, RabbitMQ, or Azure Service Bus, stronger ordering guarantees may be implemented at the partition or queue level.

Such guarantees will remain infrastructure-specific and will not change the default architectural assumption of at-least-once delivery.

## Consequences

Positive:

- Establishes realistic and explicit messaging guarantees.
- Aligns architecture with common distributed systems practices.
- Prevents incorrect assumptions about exactly-once delivery.
- Reinforces the use of idempotent consumers and Inbox pattern.

Negative:

- Consumers must handle duplicates explicitly.
- Strict ordering cannot be assumed across the system.
- Some workflows may require additional compensation logic when order matters.
