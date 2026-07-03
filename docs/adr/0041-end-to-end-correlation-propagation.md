# ADR-0041: End-to-End Correlation Propagation

## Status

Implemented

## Context

The system already standardizes event metadata and basic correlation fields through ADR-0032 (Event Metadata and Correlation Strategy). Outbox messages persist `CorrelationId`, `CausationId`, and `TraceId`, and integration events can carry message identity across asynchronous boundaries.

However, correlation was previously only partially implemented. While producer-side metadata persistence existed, the architecture did not yet consistently restore and propagate correlation context across all execution boundaries, especially during integration event consumption.

Without explicit end-to-end propagation, correlation becomes fragmented across modules and asynchronous workflows. This weakens observability, makes distributed debugging more difficult, and breaks causation chains when consumers trigger downstream commands or publish new integration events.

This concern becomes more important as the system adopts stronger messaging guarantees such as outbox delivery, inbox-based idempotency, poison message handling, and modular cross-boundary communication.

## Decision

The system adopts an end-to-end correlation propagation strategy that preserves correlation metadata across synchronous and asynchronous boundaries.

The design ensures that:

- every externally initiated request starts or resumes a correlation context
- every produced integration event carries correlation metadata
- every consumed integration event restores correlation context before application handling
- every downstream command or integration event generated from an existing flow preserves the same `CorrelationId`
- every causally triggered step sets the consumed message identifier as the new `CausationId`
- trace information is propagated when available, without making distributed tracing infrastructure mandatory

The propagation model is:

1. **Request origin**
   - An inbound HTTP request creates or resumes a correlation context.
   - If a correlation identifier already exists, it is reused.
   - Otherwise, a new correlation identifier is generated.

2. **Application execution**
   - Commands and handlers execute within the active correlation context.
   - Domain events raised during the same flow remain part of the same correlation chain.

3. **Outbox persistence**
   - Integration events persisted to the outbox copy correlation metadata from the active correlation context.
   - If no active context exists, fallback generation rules from ADR-0032 apply.

4. **Event publication and consumption**
   - Published integration events carry correlation metadata.
   - Consumers restore correlation context from the incoming integration event before invoking application logic.

5. **Downstream causation**
   - When a consumer triggers downstream commands or publishes new integration events, the original `CorrelationId` is preserved.
   - The consumed message `MessageId` becomes the new `CausationId`.

## Implementation

The implemented solution consists of the following elements:

- a correlation context accessor based on async-flow-local storage
- integration event metadata that includes message identity and correlation fields
- outbox message persistence that stores `CorrelationId`, `CausationId`, and `TraceId`
- outbox message creation that reads correlation metadata from the active context
- inbox pipeline behavior that:
  - enforces idempotent consumption through the inbox store
  - restores correlation context before invoking the actual handler
  - sets the consumed message identifier as the downstream `CausationId`

This ensures that correlation is preserved both:

- from request to produced integration event
- from consumed integration event to downstream processing and publication

## Consequences

### Positive

- Improves observability across module and service boundaries
- Enables reconstruction of end-to-end execution chains
- Preserves causation across asynchronous boundaries
- Supports distributed debugging and incident analysis
- Aligns inbox and outbox behavior under a single correlation model
- Creates a clean foundation for future OpenTelemetry integration

### Negative

- Adds infrastructure complexity around context restoration and propagation
- Requires correlation metadata to be present on integration event contracts or transport envelopes
- Increases testing surface for asynchronous and multi-hop workflows
- May require adapter work for specific brokers or transports

## Scope

This ADR defines and implements the application-level propagation model for correlation metadata inside the modular monolith architecture and its inbox/outbox messaging flow.

Transport-specific header mapping and external broker adapters may still evolve separately, but they must preserve the same architectural semantics defined here.

## Alternatives Considered

### 1. Keep correlation limited to producer-side outbox metadata only

Rejected because it does not restore correlation after asynchronous boundaries and therefore cannot support full execution-chain tracing.

### 2. Rely only on distributed tracing tools

Rejected because application-level correlation must remain available even when tracing infrastructure is absent, disabled, or partially configured.

### 3. Generate a new correlation identifier per boundary

Rejected because it breaks causal continuity and makes end-to-end analysis significantly harder.

## Related ADRs

- ADR-0032: Event Metadata and Correlation Strategy
- ADR-0033: Event Ordering and Delivery Guarantees
- ADR-0034: Dead-Letter and Poison Message Handling
- ADR-0035: Consumer Idempotency Strategy
- ADR-0039: Separate Integration Event from Inbox Message
- ADR-0040: Module-Level Registration of Integration Event Contract Assemblies for Outbox Resolution
