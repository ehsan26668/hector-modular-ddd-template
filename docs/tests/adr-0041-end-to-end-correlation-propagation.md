# Test Plan: ADR-0041 End-to-End Correlation Propagation

## Status

Accepted

## Context

This test plan validates the **end-to-end correlation propagation mechanism** described in [ADR-0041](/docs/adr/0041-end-to-end-correlation-propagation.md).  
This ADR is critical because the system relies on correlation metadata to maintain traceability across synchronous requests and asynchronous messaging boundaries. Without reliable propagation of correlation context, execution chains across modules and integration events become fragmented, making observability, incident analysis, and distributed debugging significantly more difficult.

The architecture already defines correlation metadata fields (`CorrelationId`, `CausationId`, and `TraceId`) through ADR-0032. ADR-0041 ensures these identifiers are correctly created, propagated, restored, and preserved across request handling, outbox persistence, event publication, and event consumption.

Validating this behavior is essential for ensuring that asynchronous workflows maintain causal continuity and that every step of a distributed execution chain can be reconstructed reliably within the modular DDD architecture.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on correlation context creation, propagation rules, and metadata copying logic.
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Messaging.UnitTests`

- **Integration Tests:**
  - Focus on request pipeline behavior, outbox persistence, and inbox consumption restoring correlation context.
  - Target Project: `tests/IntegrationTests/Hector.BuildingBlocks.Messaging.IntegrationTests`

---

## 1. Scope

- **Included:**
  - Correlation context creation for inbound requests
  - Propagation of correlation metadata to integration events
  - Persistence of correlation metadata in outbox messages
  - Restoration of correlation context during event consumption
  - Preservation of correlation across downstream commands and events
  - Handling missing or invalid correlation metadata

- **Excluded:**
  - External distributed tracing infrastructure (e.g., OpenTelemetry exporters)
  - Broker-specific header mapping
  - Network transport behavior

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_CreateCorrelationContext_When_RequestStartsWithoutExistingCorrelation

**Scenario:**  
An inbound request arrives without an existing correlation identifier.

**Arrange:**

- Setup request pipeline without correlation headers.
- Ensure no active correlation context exists.

**Act:**

- Execute a command through the application pipeline.

**Assert:**

- Verify a new `CorrelationId` is generated.
- Verify the correlation context becomes active for the request lifecycle.
- Verify downstream operations access the same correlation identifier.

---

### TC-02: Should_ReuseExistingCorrelationId_When_RequestContainsCorrelationHeader

**Scenario:**  
An inbound request contains an existing correlation identifier.

**Arrange:**

- Setup request pipeline with a predefined `CorrelationId`.

**Act:**

- Execute a command through the application pipeline.

**Assert:**

- Verify the existing `CorrelationId` is reused.
- Verify no new identifier is generated.
- Verify the correlation context remains consistent across the request lifecycle.

---

### TC-03: Should_CopyCorrelationMetadata_When_PersistingOutboxMessage

**Scenario:**  
An integration event is produced during command execution and persisted to the outbox.

**Arrange:**

- Establish an active correlation context.
- Trigger domain logic that produces an integration event.

**Act:**

- Persist the event to the outbox.

**Assert:**

- Verify the outbox message stores the active `CorrelationId`.
- Verify `CausationId` is set according to the current execution step.
- Verify `TraceId` is copied when available.

---

### TC-04: Should_RestoreCorrelationContext_When_IntegrationEventIsConsumed

**Scenario:**  
A consumer receives an integration event containing correlation metadata.

**Arrange:**

- Prepare an integration event with `CorrelationId`, `CausationId`, and `MessageId`.
- Ensure no active correlation context exists before consumption.

**Act:**

- Process the event through the inbox pipeline.

**Assert:**

- Verify the correlation context is restored before handler execution.
- Verify the restored `CorrelationId` matches the event metadata.
- Verify the consumer handler executes within the restored context.

---

### TC-05: Should_PreserveCorrelationId_When_ConsumerProducesDownstreamEvents

**Scenario:**  
A consumer processes an integration event and produces another integration event or command.

**Arrange:**

- Prepare a consumed integration event containing correlation metadata.

**Act:**

- Execute consumer logic that produces a downstream integration event.

**Assert:**

- Verify the downstream event preserves the original `CorrelationId`.
- Verify the consumed event `MessageId` becomes the new `CausationId`.

---

### TC-06: Should_GenerateFallbackCorrelation_When_NoActiveContextExistsDuringOutboxCreation

**Scenario:**  
An integration event is persisted without an active correlation context.

**Arrange:**

- Disable or clear the active correlation context.

**Act:**

- Persist an integration event to the outbox.

**Assert:**

- Verify fallback correlation generation rules from ADR-0032 are applied.
- Verify the outbox message still contains valid correlation metadata.

---

### TC-07: Should_PropagateTraceInformation_When_TraceIdIsAvailable

**Scenario:**  
Trace information exists during request execution and must propagate across asynchronous boundaries.

**Arrange:**

- Establish an active correlation context containing a `TraceId`.

**Act:**

- Produce and persist an integration event.

**Assert:**

- Verify the `TraceId` is copied to the outbox message metadata.
- Verify the same `TraceId` is restored during event consumption.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that no sensitive information, stack traces, connection strings, or internal IDs leak to external layers or clients.
- Verify that correlation metadata does not expose internal implementation details.

### 3.2 Observability & Traceability

- Verify that correlation identifiers appear consistently in logs across request and event boundaries.
- Verify that distributed execution chains can be reconstructed using correlation metadata.

### 3.3 Contract Stability

- Verify that integration event contracts preserve correlation metadata fields.
- Verify that correlation identifiers remain consistent across asynchronous boundaries.

---

## 4. Test Data

- **Inputs:**
  - HTTP request without correlation header
  - HTTP request with `CorrelationId`
  - Integration event containing correlation metadata
  - Integration event without correlation metadata
  - Integration event with trace information

- **Expected Outputs:**
  - Newly generated correlation context
  - Reused correlation context
  - Outbox messages containing correlation metadata
  - Restored correlation context during event consumption
  - Correct causation chains across downstream events

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED**
   - Define failing tests for correlation context creation.
   - Define failing tests for correlation propagation to outbox messages.
   - Define failing tests for context restoration during event consumption.

2. **GREEN**
   - Implement correlation context accessor.
   - Implement metadata propagation to integration events and outbox messages.
   - Implement correlation restoration inside the inbox pipeline.

3. **REFACTOR**
   - Simplify correlation propagation logic.
   - Improve separation between infrastructure and application layers.
   - Ensure thread-safe context propagation across async flows.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Security and non-functional points are verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Messaging.UnitTests/
 │       └── Correlation/
 │           ├── CorrelationContextTests.cs
 │           └── CorrelationPropagationTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Messaging.IntegrationTests/
         └── CorrelationEndToEndTests.cs
```

## Summary

This test plan ensures that ADR-0041 is validated against the expected architectural and runtime behavior.
The result should improve system quality, reliability, and maintainability while preserving the modular boundaries defined by the architecture.
