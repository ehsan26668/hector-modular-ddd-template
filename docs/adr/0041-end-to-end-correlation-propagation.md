# ADR-0041: End-to-End Correlation Propagation

- **Status:** Proposed
- **Date:** 2026-06-16

## Context

The system already standardizes event metadata and basic correlation fields through ADR-0032 (Event Metadata and Correlation Strategy). Outbox messages can persist `CorrelationId`, `CausationId`, and `TraceId`, and HTTP-originated requests can establish an initial correlation context.

However, the current design does not yet define a complete end-to-end propagation strategy across all message boundaries and execution flows. In particular, the architecture still needs an explicit policy for how correlation metadata should move through:

- inbound HTTP requests
- application commands
- domain events
- integration events
- outbox persistence
- event publication
- inbox processing
- integration event consumers
- downstream command handling

Without a dedicated ADR, correlation may remain partial and inconsistent across modules and process boundaries. This would reduce observability, make distributed debugging difficult, and limit traceability of causation chains in asynchronous workflows.

This need becomes more important as the architecture evolves toward stronger guarantees for event ordering, delivery handling, idempotent consumption, and cross-module communication.

## Decision

The system will adopt an end-to-end correlation propagation strategy that preserves correlation metadata across synchronous and asynchronous boundaries.

The design will ensure that:

- every externally initiated request starts or resumes a correlation context
- every produced integration event carries correlation metadata
- every consumed integration event restores correlation context before application handling
- every downstream command or event generated from an existing flow preserves the same `CorrelationId`
- every causally triggered step assigns the triggering message identifier as `CausationId`
- trace information can be propagated when available, without making tracing infrastructure mandatory

The intended propagation model is:

1. **Request origin**
   - An inbound HTTP request creates or resumes a correlation context.
   - If a correlation header already exists, it is reused.
   - Otherwise, a new correlation identifier is generated.

2. **Application execution**
   - Commands and handlers execute within the active correlation context.
   - Domain events raised during the same flow are considered part of the same correlation chain.

3. **Outbox persistence**
   - Integration events persisted to the outbox include correlation metadata copied from the active context.
   - If no active context exists, fallback rules defined in ADR-0032 continue to apply.

4. **Event publication and consumption**
   - Published integration events carry correlation metadata.
   - Consumers restore the correlation context from the incoming message before invoking application logic.

5. **Downstream causation**
   - If a consumer triggers new commands or produces new integration events, the original `CorrelationId` is preserved.
   - The consumed message identifier becomes the new `CausationId`.

## Consequences

### Positive

- Improves observability across module and service boundaries
- Enables reconstruction of end-to-end execution chains
- Supports distributed debugging and incident analysis
- Creates a clean foundation for future OpenTelemetry integration
- Aligns correlation behavior between producers and consumers
- Reinforces consistency of inbox/outbox-based messaging workflows

### Negative

- Adds infrastructure complexity around context restoration and propagation
- Requires clear contracts for correlation metadata on incoming and outgoing messages
- Increases testing surface for asynchronous and multi-hop workflows
- May require future adapter work for specific brokers or transports

## Scope

This ADR defines the architectural direction and propagation rules, but does not by itself implement all required infrastructure.

Expected implementation work will likely depend on or interact with:

- ADR-0033: Event Ordering and Delivery Guarantees
- ADR-0034: Dead-Letter and Poison Message Handling
- ADR-0035: Consumer Idempotency Strategy

## Implementation Notes

The future implementation is expected to include:

- correlation restoration at consumer/inbox entry points
- propagation rules for message publication
- standardized correlation headers or transport metadata mapping
- optional trace identifier bridging with distributed tracing mechanisms
- automated tests for multi-step correlation chains

## Alternatives Considered

### 1. Keep correlation limited to producer-side outbox metadata only

This was rejected because it does not support full-chain tracing after asynchronous boundaries.

### 2. Rely only on distributed tracing tools

This was rejected because application-level correlation must remain available even when full tracing infrastructure is absent or partially configured.

### 3. Generate a new correlation identifier per boundary

This was rejected because it breaks causal continuity and makes end-to-end analysis significantly harder.

## Related ADRs

- ADR-0032: Event Metadata and Correlation Strategy
- ADR-0033: Event Ordering and Delivery Guarantees
- ADR-0034: Dead-Letter and Poison Message Handling
- ADR-0035: Consumer Idempotency Strategy
