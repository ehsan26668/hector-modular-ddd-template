# ADR 0023: Adopt Inbox Pattern for Idempotent Event Handling

## Status

Accepted

## Context

ADR‑0021 introduced the Transactional Outbox pattern to guarantee reliable persistence of domain events.

ADR‑0022 introduced a background Outbox Processor responsible for reading pending outbox messages and publishing them through the application’s messaging infrastructure.

While the outbox ensures that events are reliably produced, it does not guarantee that events are processed only once by consumers.

In distributed and asynchronous systems, duplicate delivery can occur due to:

- message broker retries
- network failures
- consumer crashes during processing
- manual replay of events
- outbox retry mechanisms

Without protection against duplicates, event handlers may execute multiple times and cause inconsistent state changes such as:

- duplicated records
- repeated side effects
- incorrect aggregate updates

To ensure safe event processing, consumers must be able to detect and ignore duplicate messages.

## Decision

We will adopt the Inbox Pattern to guarantee idempotent event handling.

The Inbox pattern introduces a persistent store that records processed message identifiers before executing event handlers.

Each incoming event message will be processed according to the following workflow:

1. The message identifier is checked against the Inbox table.
2. If the message has already been processed, the handler execution is skipped.
3. If the message is new:

    - the message identifier is stored in the Inbox table
    - the event handler is executed
4. The transaction commits only after both the Inbox record and the handler side effects are persisted.

This guarantees that each event is processed at most once by a given consumer.
The Inbox record will typically contain:
`Id`
`MessageId`
`ProcessedOn`
`Consumer`
The `MessageId` corresponds to the identifier stored in the Outbox message.

The Inbox mechanism will be implemented in the infrastructure layer and integrated into the event handling pipeline so that application handlers remain unaware of the deduplication mechanism.

## Consequences

Positive:

- Guarantees idempotent event handling
- Prevents duplicate side effects
- Enables safe retries and message replay
- Complements the Transactional Outbox pattern
- Supports future integration with message brokers
- Keeps domain and application layers infrastructure‑agnostic

Negative:

- Introduces an additional persistence table
- Slightly increases storage requirements
- Adds additional database checks during message processing
- Requires periodic cleanup of old inbox records
