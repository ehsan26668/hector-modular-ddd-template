# ADR 0032: Event Metadata and Correlation Strategy

## Status

Implemented  
Date: 2026-06-16

## Context

The system uses domain events and integration events as part of its event-driven architecture.

Events are persisted using the Transactional Outbox pattern and later published by the outbox processor.

As the system grows, it becomes increasingly important to trace how events flow through the system. In distributed or modular systems, it must be possible to answer questions such as:

- Which request produced this event?
- Which event triggered another event?
- Which service or module produced the event?
- When did the event occur?

Without standardized event metadata, debugging distributed workflows becomes difficult and observability suffers.

To support monitoring, tracing, and debugging, events should include consistent metadata describing their origin and relationships.

## Decision

All events published by the system will include standardized metadata fields.

These metadata fields enable event correlation, tracing, and observability across modules and services.

### Standard Event Metadata

The following metadata fields will be associated with each event:

EventId

A unique identifier for the event instance.

Example:

    Guid EventId

CorrelationId

Represents the logical operation or request that produced the event chain. Multiple events triggered by the same request share the same correlation identifier.

Example:

    Guid CorrelationId

CausationId

Represents the event or command that directly caused this event.

Example:

    Guid? CausationId

OccurredOn

Timestamp representing when the event occurred.

Example:

    DateTime OccurredOn

Producer

Identifies the module or service that produced the event.

Example:

    string Producer

Version

Represents the event contract version.

Example:

    int Version

### Metadata Storage

Metadata may be stored in one of two ways depending on infrastructure:

1. Embedded in the event payload
2. Stored in message headers when using a message broker

The outbox message may also store metadata fields separately to simplify querying and diagnostics.

### Metadata Propagation

When a command triggers an event:

- A new EventId must be generated.
- CorrelationId should propagate from the incoming request.
- CausationId should reference the triggering command or event.

Example flow:

    HTTP Request
        ↓
    Command (CorrelationId = X)
        ↓
    Domain Event (CorrelationId = X, CausationId = CommandId)
        ↓
    Integration Event (CorrelationId = X, CausationId = DomainEventId)

This ensures the full causal chain can be reconstructed.

### Observability Integration

The metadata strategy is compatible with distributed tracing systems such as:

- OpenTelemetry
- Application Insights
- Jaeger
- Zipkin

Correlation identifiers may map directly to trace identifiers in observability tools.

## Consequences

Positive:

- Enables end-to-end tracing of event flows.
- Improves debugging in distributed or modular architectures.
- Supports observability and monitoring tools.
- Provides clear causal relationships between commands and events.

Negative:

- Adds additional metadata fields to events.
- Requires propagation of correlation identifiers through the system.
- Slightly increases message size.
