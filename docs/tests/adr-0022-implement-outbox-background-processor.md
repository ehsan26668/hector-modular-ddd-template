# Test Plan: ADR‑0022 Implement Outbox Background Processor

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR‑0022](/docs/adr/0022-outbox-background-processor.md): *Implement Outbox Background Processor*.

[ADR‑0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md) introduced the Transactional Outbox pattern to ensure domain events are persisted transactionally. [ADR‑0022](/docs/adr/0022-outbox-background-processor.md) completes the pattern by adding a Background Processor that reliably:

- loads unprocessed outbox messages
- deserializes them
- publishes them
- marks them as processed
- retries safely

This test plan ensures the processor is correct, safe, idempotent, observable, and reliable.

It validates batch handling, safe concurrency, durable retry behavior, error propagation, and correct state updates to Outbox records.

## Test Strategy

### Unit Tests

#### Validate isolated logic such as

- batch selection
- message publishing
- retry behavior
- marking messages processed
- deserialization

Target project:

- tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests

### Integration Tests

#### Validate end‑to‑end outbox processing against real EF Core DbContext

- correct DB reads
- correct state transitions
- concurrency correctness
- background service lifecycle

Target project:

- tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests

---

## 1. Scope

List exactly what is included and excluded from this test plan to set clear boundaries for the validation process.

### Included

- Selecting pending outbox messages (ProcessedOn == null)
- Deserialization of event payloads
- Publishing via `IOutboxPublisher`
- Marking messages as processed
- Batch processing behavior
- Safe retries on failure
- Ensuring no message is lost
- Background service periodic execution
- Idempotency guarantees at the outbox level
- Concurrency safety (no double-processing)

### Excluded

- External message broker integration
- Inbox pattern (handled in separate ADR)
- Dead-letter queue strategies (covered by [ADR‑0034](/docs/adr/0034-dead-letter-and-poison-message-handling.md))
- Specific serializer implementation details (covered by serialization ADRs)
- Infrastructure-level metrics storage (only behavior observed)

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_LoadPendingMessages_When_BackgroundProcessorRuns

**Scenario:**

- Processor must retrieve messages where ProcessedOn is null.

**Arrange:**

- Create multiple outbox messages, some processed, some not.

**Act:**

- Run processor iteration.

**Assert:**

- Only unprocessed messages are loaded.

---

### TC-02

- #### Should_DeserializeEventPayload_When_ProcessingMessage

**Scenario:**

- Serialized JSON payload must convert back to the correct event type.

**Arrange:**

- Create an outbox message with known event type + payload.

**Act:**

- Run `Deserialize()` logic.

**Assert:**

- Deserialized event matches expected structure.

---

### TC-03

- #### Should_PublishEvent_When_MessageIsProcessed

**Scenario:**

- Processor uses publisher to publish the event.

**Arrange:**

- Mock `IOutboxPublisher`.

**Act:**

- Run processor.

**Assert:**

- Publisher is called exactly once per message.

---

### TC-04

- #### Should_MarkMessageAsProcessed_When_PublicationSucceeds

**Scenario:**

- After publishing, the outbox record must be marked processed.

**Arrange:**

- Create pending outbox message.

**Act:**

- Run processor.

**Assert:**

- `ProcessedOn` is non-null.

---

### TC-05

- #### Should_NotMarkAsProcessed_When_PublicationFails

**Scenario:**

- Failure should NOT mark message as processed.

**Arrange:**

- Mock publisher to throw exception.

**Act:**

- Run processor.

**Assert:**

- Message remains unprocessed.

---

### TC-06

- #### Should_RetryFailedMessages_OnNextExecution

**Scenario:**

- Unprocessed messages remain retryable.

**Arrange:**

- Create failing message scenario on first run.

**Act:**

- Run processor twice.

**Assert:**

- Second attempt publishes successfully.

---

### TC-07

- #### Should_ProcessMessagesInBatches_When_ManyMessagesExist

**Scenario:**

- Processor must limit batch size.

**Arrange:**

- Insert > batch size pending messages (e.g., 100).

**Act:**

- Run processor.

**Assert:**

- Only batch-sized subset is processed per iteration.

---

### TC-08

- #### Should_BeConcurrencySafe_When_TwoProcessorsRunSimultaneously

**Scenario:**

- Prevent double-processing.

**Arrange:**

- Start two processors concurrently.

**Act:**

- Process the same set of pending messages.

**Assert:**

- No message is processed more than once.

---

### TC-09

- #### Should_RunPeriodically_When_BackgroundServiceIsActive

**Scenario:**

- Background service must trigger processor repeatedly.

**Arrange:**

- Start `OutboxProcessorBackgroundService`.

**Act:**

- Wait for two execution cycles.

**Assert:**

- Processor is invoked at least twice.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- No sensitive data written to logs during outbox processing.
- No exposure of internal stack traces or event details.

### 3.2 Observability & Traceability

- Logging includes message ID + event type.
- Correlation ID flows from event metadata into logs.
- Failures are logged with structured metadata.

### 3.3 Contract Stability

- Outbox message format and serialization remain compatible over time.
- Processing state transitions remain backward compatible.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Sample domain events (`ProjectCreatedDomainEvent`, etc.)
  - Serialized payloads
  - Pending and processed OutboxMessage records
  - Artificial failures (exceptions in publisher)

- **Expected Outputs:**
  - Correctly deserialized event object
  - Processed messages with timestamps
  - Unprocessed messages retained on failure
  - No duplicate processed records

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Write initial failing tests for batch selection, publishing, and processing logic.

2. **GREEN:**  
   Implement outbox processor, publisher behavior, and processing rules.

3. **REFACTOR:**  
   Optimize batching, logging, and error handling without breaking tests.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass
- [ ] All Integration Tests pass
- [ ] Concurrency correctness verified
- [ ] No message loss occurs under failure
- [ ] Documentation updated

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       └── Outbox/
 │           ├── OutboxProcessorTests.cs
 │           ├── OutboxPublisherTests.cs
 │           ├── OutboxDeserializationTests.cs
 │           └── OutboxBatchingTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         ├── OutboxProcessorTests.cs
         ├── OutboxConcurrentProcessingTests.cs
         └── OutboxEndToEndProcessingTests.cs
```

---

## Summary

This test plan ensures that the Outbox Background Processor provides reliable, safe, idempotent, and observable message delivery. It validates the entire lifecycle of a domain event after persistence—retrieval, deserialization, publication, retry, and marking processed—guaranteeing the durability and reliability required in a modular DDD system.

---
