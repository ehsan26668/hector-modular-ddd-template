# ADR 0022: Implement Outbox Background Processor

## Status

Accepted

## Context

ADR‑0021 introduced the Transactional Outbox pattern to guarantee reliable persistence of domain events within the same database transaction as aggregate state changes.

Domain events are now stored in the OutboxMessages table during DbContext.SaveChangesAsync.

However, persisting events alone is not sufficient. A separate component must be responsible for:

- reading pending messages from the outbox
- publishing them to the application messaging infrastructure
- marking them as processed
- retrying failures safely

Without such a processor, the outbox would grow indefinitely and domain events would never be delivered.

The processor must also satisfy several production requirements:

- safe concurrent processing
- retry capability
- idempotent delivery
- minimal database locking
- observability and logging

## Decision

We will implement a background outbox processor inside the infrastructure layer.

The processor will:

1. Periodically poll the `OutboxMessages` table.
2. Retrieve messages where `ProcessedOn` is null.
3. Deserialize the stored domain event.
4. Publish the event using the application’s mediator infrastructure.
5. Mark the message as processed by setting `ProcessedOn`.

The processor will run as a BackgroundService.

Processing will occur in batches to avoid long transactions and reduce database pressure.

## Consequences

Positive:

- Guarantees eventual delivery of domain events
- Prevents event loss during transaction failures
- Keeps domain layer independent from messaging infrastructure
- Allows horizontal scaling of processors
- Enables retries and dead‑letter strategies

Negative:

- Adds operational complexity
- Requires monitoring of the outbox table
- Events are delivered asynchronously instead of immediately

## Implementation Notes

The processor will be implemented with the following components:

- OutboxProcessorBackgroundService
- IOutboxPublisher
- OutboxPublisher
- batch processing logic
- JSON deserialization using the stored event type

The processor will initially use the internal mediator to dispatch events, but the architecture allows replacing it with a message broker in the future.
