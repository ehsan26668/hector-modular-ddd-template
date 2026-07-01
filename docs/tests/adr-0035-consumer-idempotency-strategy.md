# Test Plan: ADR-0035 Consumer Idempotency Strategy

## Status

Accepted

## Context

This test plan validates the **Consumer Idempotency Strategy** described in [ADR-0035](/docs/adr/0035-consumer-idempotency-strategy.md).

The system follows an **event-driven architecture** where events are published using the **Transactional Outbox pattern** and consumed using the **Inbox pattern**.

Because the messaging system provides **at-least-once delivery guarantees**, duplicate message deliveries are expected under several operational scenarios:

- transient failures during event publishing
- retries performed by the outbox processor
- consumer restarts
- message redelivery by messaging infrastructure
- operational replay of events

Without idempotent consumer behavior, duplicate deliveries may cause severe business inconsistencies such as:

- repeated state changes
- duplicate projections
- duplicated notifications
- corrupted read models
- repeated financial transactions

ADR‑0035 mandates that **all event consumers must be idempotent**, ensuring that processing the same event multiple times produces the same result as processing it once.

The recommended mechanism for achieving idempotency is the **Inbox pattern**, which records processed messages and prevents duplicate side effects.

This test plan verifies the correctness and reliability of the consumer idempotency strategy implemented in the system.

---

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests**
  - Validate idempotency behavior inside consumer pipelines.
  - Verify inbox checks, duplicate detection, and handler behavior.
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Application.UnitTests`

- **Integration Tests**
  - Validate end-to-end idempotent processing across message handling and inbox persistence.
  - Target Project: `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

- **Architecture Tests**
  - Validate structural constraints enforcing idempotent consumer design.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

### Included

- Inbox pattern enforcement for event consumers
- Duplicate event detection
- Prevention of duplicate side effects
- Recording processed messages in inbox store
- Safe handling of duplicate message deliveries
- Stable event identifiers
- Consumer identifier tracking

### Excluded

- External broker deduplication
- Exactly-once delivery guarantees
- Business-specific projection logic

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_RecordMessageInInbox_When_EventIsProcessed

**Scenario:**  
When an event is processed successfully, the message must be recorded in the inbox store.

**Arrange:**

- Create a test integration event
- Configure `InboxBehavior` with an empty inbox store

**Act:**

- Execute event handler through the mediator pipeline

**Assert:**

- Inbox store contains a record for the processed message
- Message identifier is persisted
- Processing timestamp is recorded

---

### TC-02: Should_SkipProcessing_When_MessageAlreadyExistsInInbox

**Scenario:**  
If a message has already been processed, the consumer must skip execution.

**Arrange:**

- Insert an inbox record for the message
- Configure event handler

**Act:**

- Process the same event again

**Assert:**

- Handler is not executed
- No side effects occur
- Processing returns successfully

---

### TC-03: Should_TreatDuplicateMessages_AsSuccessfulNoOp

**Scenario:**  
Duplicate message deliveries must be treated as successful no‑op operations.

**Arrange:**

- Process an event once
- Store inbox entry

**Act:**

- Deliver the same message again

**Assert:**

- No duplicate state changes occur
- Handler execution is skipped
- Operation completes without error

---

### TC-04: Should_RecordConsumerIdentifier_ForProcessedMessages

**Scenario:**  
Inbox records must include the consumer identifier.

**Arrange:**

- Configure `IInboxConsumerNameProvider`
- Process an event

**Act:**

- Inspect stored inbox message

**Assert:**

- Consumer identifier is persisted
- Consumer identity matches handler

---

### TC-05: Should_PreventDuplicateSideEffects_When_MessageDeliveredMultipleTimes

**Scenario:**  
Repeated delivery of the same message must not cause duplicated side effects.

**Arrange:**

- Create event handler with observable side effect
- Process event once

**Act:**

- Deliver same event multiple times

**Assert:**

- Side effect occurs only once
- Subsequent deliveries are ignored

---

### TC-06: Should_AllowDifferentConsumers_ToProcessSameEvent

**Scenario:**  
Different consumers must be able to process the same event independently.

**Arrange:**

- Configure two separate consumers
- Process the same event

**Act:**

- Execute both handlers

**Assert:**

- Each consumer records its own inbox entry
- Each handler executes once

---

### TC-07: Should_PersistInboxRecords_BeforeCompletingProcessing

**Scenario:**  
Inbox records must be persisted during processing to ensure safe deduplication.

**Arrange:**

- Process event through inbox pipeline

**Act:**

- Inspect inbox store after processing

**Assert:**

- Inbox record exists
- Message identifier matches event metadata

---

### TC-08: Should_HandleConcurrentDuplicateDeliverySafely

**Scenario:**  
Concurrent delivery of duplicate messages must not result in duplicate processing.

**Arrange:**

- Simulate concurrent event handling

**Act:**

- Process the same event concurrently

**Assert:**

- Only one processing succeeds
- Inbox store prevents duplicate execution

---

### TC-09: Should_AllowEventReplayWithoutSideEffects

**Scenario:**  
Operational event replay must not produce duplicate business effects.

**Arrange:**

- Process event once
- Replay same event

**Act:**

- Execute handler again

**Assert:**

- Replay is ignored
- No duplicate mutations occur

---

## 3. Non-Functional Validation Points

### 3.1 Reliability

- Verify duplicate deliveries do not corrupt system state.
- Verify inbox store ensures consistent deduplication.

### 3.2 Observability

- Verify inbox records enable operational diagnostics.
- Verify correlation metadata remains traceable across processing.

### 3.3 Performance

- Verify inbox lookup does not significantly impact event processing throughput.
- Verify deduplication checks scale under load.

---

## 4. Test Data

### Inputs

- Unique integration event messages
- Duplicate event deliveries
- Concurrent event deliveries
- Multiple consumer handlers

### Expected Outputs

- Inbox records created for processed messages
- Duplicate messages ignored
- No repeated side effects
- Separate consumers process events independently

---

## 5. TDD Execution Plan

### 1. RED

- Write failing tests validating duplicate detection.
- Write tests verifying inbox persistence and consumer identity tracking.

### 2. GREEN

- Implement inbox checks inside `InboxBehavior`.
- Implement message recording in `IInboxStore`.

### 3. REFACTOR

- Extract reusable test helpers for inbox messages.
- Improve handler pipeline composition.
- Ensure separation between deduplication and business logic.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Architecture rules verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Application.UnitTests/
 │       └── Messaging/
 │           ├── InboxBehaviorTests.cs
 │           ├── InboxCorrelationBehaviorTests.cs
 │           └── InboxPipelineBehaviorTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.BuildingBlocks.Persistence.IntegrationTests/
 │       └── InboxIdempotencyTests.cs
 │
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         └── ConsumerIdempotencyArchitectureTests.cs
```

## Summary

This test plan ensures that [ADR‑0035](/docs/adr/0035-consumer-idempotency-strategy.md) enforces safe event processing under **at-least-once delivery guarantees**.

By implementing inbox-based deduplication and validating idempotent consumer behavior, the system prevents duplicate side effects, supports event replay scenarios, and maintains consistent system state across distributed workflows.
