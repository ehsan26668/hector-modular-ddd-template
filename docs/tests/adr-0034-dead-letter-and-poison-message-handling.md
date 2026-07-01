# Test Plan: ADR-0034 Dead Letter and Poison Message Handling

## Status

Accepted

## Context

This test plan validates the **Dead Letter and Poison Message Handling Strategy** described in [ADR-0034](/docs/adr/0034-dead-letter-and-poison-message-handling.md).

The system uses the **Transactional Outbox pattern** to persist integration events before publishing them to the message bus. The outbox processor asynchronously publishes these messages and applies retry policies.

Retry logic improves resilience for **transient failures**, such as:

- temporary network interruptions
- database connectivity issues
- broker availability problems

However, some failures are **permanent**, including:

- invalid payloads
- deserialization failures
- unsupported event types
- contract mismatches
- handler logic errors

Without a poison message strategy, these failures may cause **infinite retry loops**, noisy logs, and hidden operational issues.

ADR‑0034 introduces a **dead-letter strategy** where permanently failing messages are identified and marked as poison once the configured retry limit is reached.

Poison messages must:

- stop participating in normal outbox processing
- preserve failure diagnostics
- remain available for inspection and manual recovery

This behavior is critical for ensuring **reliable event publishing, operational visibility, and system stability** within the modular DDD architecture.

---

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on retry logic, poison detection rules, and state transitions inside the outbox processing pipeline.
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Focus on end-to-end outbox processing behavior including retry handling, persistence, and message filtering.
  - Target Project: `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

### Included

- Retry count tracking in `OutboxMessage`
- Retry limit enforcement through `OutboxProcessingPolicy`
- Poison detection when `RetryCount >= MaxRetryCount`
- Transition of messages to poison state
- Persistence of failure metadata (`FailedOn`, `FailureReason`, `IsPoisoned`)
- Exclusion of poison messages from outbox processing queries
- Preservation of poison messages for operational diagnostics

### Excluded

- External broker dead-letter queues
- Automated replay tooling
- Monitoring dashboards and alert integrations

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_IncrementRetryCount_When_PublishFails

**Scenario:**  
When an integration event fails to publish, the retry counter must increment.

**Arrange:**

- Create an `OutboxMessage` with `RetryCount = 0`
- Mock `IOutboxPublisher` to throw an exception
- Configure `MaxRetryCount` greater than `1`

**Act:**

- Execute the outbox processor

**Assert:**

- `RetryCount` increments to `1`
- Message remains eligible for retry
- `IsPoisoned == false`

---

### TC-02: Should_MarkMessageAsPoison_When_RetryLimitExceeded

**Scenario:**  
A message that continues to fail after reaching the maximum retry limit must be marked as poison.

**Arrange:**

- Create an `OutboxMessage`
- Set `RetryCount = MaxRetryCount`
- Mock `IOutboxPublisher` to throw an exception

**Act:**

- Execute the outbox processor

**Assert:**

- `IsPoisoned == true`
- `FailedOn` is populated
- `FailureReason` contains exception information

---

### TC-03: Should_StopProcessing_When_MessageIsPoison

**Scenario:**  
Poison messages must not participate in normal outbox processing.

**Arrange:**

- Insert an `OutboxMessage` with `IsPoisoned = true`

**Act:**

- Run the outbox processor cycle

**Assert:**

- Publisher is not invoked
- Message is excluded from processing query

---

### TC-04: Should_PersistFailureMetadata_When_MessageBecomesPoison

**Scenario:**  
Failure metadata must be stored when a message transitions to poison state.

**Arrange:**

- Create an outbox message
- Configure publisher to fail until retry limit is exceeded

**Act:**

- Execute outbox processor

**Assert:**

Database record contains:

- `FailedOn`
- `FailureReason`
- `IsPoisoned = true`

---

### TC-05: Should_ExcludePoisonMessages_FromOutboxProcessingQuery

**Scenario:**  
Poison messages must not be returned by normal outbox queries.

**Arrange:**

Insert:

- one pending outbox message
- one poison message

**Act:**

- Execute outbox selection query

**Assert:**

- Only the pending message is returned
- Poison message is excluded

---

### TC-06: Should_PreservePoisonMessages_ForOperationalDiagnostics

**Scenario:**  
Poison messages must remain stored for investigation and manual recovery.

**Arrange:**

- Create a poison message in the database

**Act:**

- Run several outbox processor cycles

**Assert:**

- Message remains stored
- Failure metadata remains unchanged

---

### TC-07: Should_TreatDeserializationFailure_AsPoisonCandidate

**Scenario:**  
Invalid serialized payloads must eventually transition to poison state.

**Arrange:**

- Insert outbox message with corrupted JSON payload

**Act:**

- Run outbox processor repeatedly

**Assert:**

- Processor retries message
- After retry limit the message becomes poison

---

### TC-08: Should_TreatUnsupportedEventType_AsPoisonCandidate

**Scenario:**  
Messages referencing unknown event types must eventually become poison.

**Arrange:**

- Insert outbox message referencing an event type not registered in `IOutboxEventTypeResolver`

**Act:**

- Execute outbox processor

**Assert:**

- Retry attempts occur
- Message transitions to poison state

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that exception messages stored in `FailureReason` do not expose sensitive information.
- Verify that stack traces, connection strings, or internal infrastructure details are not leaked externally.

### 3.2 Observability & Traceability

- Verify structured logging when a message transitions to poison state.
- Verify correlation metadata remains available for diagnostics.
- Ensure failed messages can be traced end-to-end.

### 3.3 Contract Stability

- Verify poison handling does not modify serialized event payloads.
- Verify event contract structure remains unchanged.
- Verify compatibility with event serialization strategy.

---

## 4. Test Data

### Inputs

- Outbox message with `RetryCount = 0`
- Outbox message with `RetryCount = MaxRetryCount`
- Corrupted serialized payload
- Unknown event type
- Publisher throwing exceptions

### Expected Outputs

- Retry count increments correctly
- Poison messages flagged with `IsPoisoned = true`
- Failure metadata persisted
- Poison messages excluded from processing queries
- Failed messages preserved for diagnostics

---

## 5. TDD Execution Plan

### 1. RED

- Write failing tests validating poison detection behavior.
- Write tests validating retry limits and metadata persistence.

### 2. GREEN

- Implement poison detection inside `OutboxProcessor`.
- Implement retry limit enforcement through `OutboxProcessingPolicy`.
- Persist failure metadata on `OutboxMessage`.

### 3. REFACTOR

- Extract reusable test builders for `OutboxMessage`.
- Centralize poison detection logic in the outbox processing pipeline.
- Improve naming and separation between retry and failure handling logic.

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
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       └── Outbox/
 │           ├── OutboxProcessorRetryTests.cs
 │           ├── OutboxProcessorPoisonMessageTests.cs
 │           ├── OutboxFailureMetadataTests.cs
 │           └── OutboxProcessingPolicyTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
     └── OutboxPoisonMessageHandlingTests.cs
```

## Summary

This test plan ensures that [ADR‑0034](/docs/adr/0034-dead-letter-and-poison-message-handling.md) is validated against the expected architectural and runtime behavior.  
By enforcing retry limits, isolating poison messages, and preserving failure diagnostics, the system improves reliability, operational visibility, and stability of the outbox processing pipeline.
