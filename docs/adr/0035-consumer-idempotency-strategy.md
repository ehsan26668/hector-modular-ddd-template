# ADR 0035: Consumer Idempotency Strategy

## Status

Proposed

## Context

The system adopts an event-driven architecture using the Transactional Outbox pattern for reliable event publishing and the Inbox pattern for idempotent event consumption.

Events may be delivered more than once due to various operational scenarios such as:

- transient failures during event publishing
- retries in the outbox processor
- consumer restarts
- message redelivery by messaging infrastructure
- manual event replay during operational recovery

Because the system guarantees **at-least-once delivery**, duplicate events must be considered a normal scenario rather than an exceptional one.

Without a clear idempotency strategy, duplicate deliveries can lead to unintended side effects such as:

- repeated state changes
- duplicate projections
- duplicate notifications
- inconsistent read models
- financial or business inconsistencies

Therefore, consumers must be designed to safely process the same event multiple times.

## Decision

All event consumers in the system must be **idempotent**.

Processing the same event more than once must produce the same business outcome as processing it once.

Consumers must not rely on the messaging infrastructure to guarantee exactly-once delivery.

### Idempotent Consumer Behavior

An idempotent consumer must:

- detect whether an event has already been processed
- avoid applying duplicate side effects
- safely ignore repeated deliveries of the same message

The recommended mechanism for implementing idempotency is the **Inbox pattern**.

### Inbox Pattern Integration

Each consumed event should be recorded in an inbox store before or during processing.

Typical inbox information includes:

- message identifier
- event type
- consumer identifier
- processing timestamp

When a message is received:

1. the system checks whether the message was already processed
2. if it was processed, the handler exits without performing side effects
3. otherwise the message is recorded and processing continues

This ensures that duplicate deliveries are handled safely.

### Idempotency Rules

Event consumers must follow these rules:

- Consumers must assume duplicate message delivery is possible.
- Handlers must avoid non-idempotent operations unless protected by deduplication.
- Event identifiers must remain stable and unique.
- Duplicate messages must be treated as successful no-ops.
- Handlers must not rely on strict event ordering.

### Examples

Safe idempotent operations include:

- updating a read model using upsert semantics
- ignoring events already recorded in the inbox table
- verifying state transitions before applying them

Unsafe operations without protection include:

- sending the same email multiple times
- charging a payment more than once
- applying the same domain mutation repeatedly
- decrementing inventory multiple times

These operations must be guarded with idempotency checks.

### Relationship to Other ADRs

This decision complements the existing messaging architecture:

- ADR-0021: Adopt Transactional Outbox for Domain Events
- ADR-0023: Adopt Inbox Pattern for Idempotent Event Handling
- ADR-0033: Event Ordering and Delivery Guarantees
- ADR-0034: Dead Letter and Poison Message Handling

Together these decisions establish a consistent reliability model for event processing.

## Consequences

Positive:

- Ensures safe processing under at-least-once delivery guarantees.
- Prevents duplicate side effects in distributed workflows.
- Improves system resilience during retries and failures.
- Supports operational scenarios such as event replay.

Negative:

- Consumers require additional persistence for deduplication.
- Handler logic becomes slightly more complex.
- Operational diagnostics may require tracking message history.
