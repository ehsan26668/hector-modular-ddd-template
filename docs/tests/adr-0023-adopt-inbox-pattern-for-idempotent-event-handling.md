# Test Plan: ADR‑0023 Adopt Inbox Pattern for Idempotent Event Handling

## Status

Accepted

## Context

This test plan validates [ADR‑0023](/docs/adr/0023-adopt-inbox-pattern-for-idempotent-event-handling.md): *Adopt Inbox Pattern for Idempotent Event Handling*.

After introducing the Transactional Outbox ([ADR‑0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md)) and Outbox Processor ([ADR‑0022](/docs/adr/0022-outbox-background-processor.md)), the system guarantees reliable production of events. However, distributed systems can still deliver messages multiple times due to:

- broker retries
- network failures
- consumer crashes
- manual replay
- outbox retry mechanisms

Without protection, duplicate deliveries can cause:

- repeated handler execution
- duplicated records
- inconsistent aggregate state
- repeated external side effects

The Inbox Pattern prevents these issues by **persistently tracking processed messages** before executing handlers.

This test plan validates that:

- duplicate messages are detected
- handlers execute only once per message
- message recording and side effects occur in the same transaction
- inbox persistence behaves correctly
- mediator pipeline integrates the behavior transparently

## Test Strategy

### Unit Tests

#### Validate isolated behavior of the inbox pipeline and store abstraction

Focus areas:

- duplicate detection
- correct invocation of handlers
- correct interactions with `IInboxStore`

Target project:

- tests/UnitTests/Hector.BuildingBlocks.Application.UnitTests
- tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests

### Integration Tests

#### Validate full pipeline behavior using EF Core persistence

Focus areas:

- persistence of inbox records
- atomic transaction behavior
- correct skipping of duplicate events

Target project:

- tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests

---

## 1. Scope

List exactly what is included and excluded from this test plan to set clear boundaries for the validation process.

### Included

- `InboxPipelineBehavior`
- `IInboxStore`
- `EfCoreInboxStore`
- `InboxMessage` entity persistence
- duplicate message detection
- mediator pipeline integration
- transactional consistency between handler and inbox persistence
- consumer identification via `IInboxConsumerNameProvider`

### Excluded

- external message brokers
- message transport mechanisms
- inbox cleanup policies (covered in later ADRs)
- outbox message production (covered in [ADR‑0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md))

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_SkipHandlerExecution_When_MessageAlreadyProcessed

**Scenario:**

- If a message identifier already exists in the inbox table, the handler must not execute.

**Arrange:**

- Mock `IInboxStore`
- Configure store to return `true` for `ExistsAsync(messageId, consumer)`

**Act:**

- Execute mediator pipeline with the same message

**Assert:**

- Handler is not executed
- No additional store writes occur

---

### TC-02

- #### Should_RecordMessage_When_MessageIsProcessedFirstTime

**Scenario:**

- When a message is processed for the first time, it must be recorded in the inbox.

**Arrange:**

- `ExistsAsync` returns `false`
- mock handler

**Act:**

- Execute mediator pipeline

**Assert:**

- handler executed exactly once
- `IInboxStore.StoreAsync()` invoked

---

### TC-03

- #### Should_ExecuteHandler_When_MessageIsNew

**Scenario:**

- A new message should pass through the pipeline and execute the handler.

**Arrange:**

- Inbox store indicates message does not exist

**Act:**

- Send event through mediator

**Assert:**

- handler executed successfully
- inbox record created

---

### TC-04

- #### Should_PersistInboxRecord_When_HandlerCompletes

**Scenario:**

- The inbox record must be persisted when handler execution succeeds.

**Arrange:**

- new message
- valid handler

**Act:**

- process message

**Assert:**

- inbox record persisted with:
  - messageId
  - consumer
  - processed timestamp

---

### TC-05

- #### Should_NotRecordInboxMessage_When_HandlerFails

**Scenario:**

- If the handler throws an exception, the inbox record should not be persisted.

**Arrange:**

- new message
- handler throws exception

**Act:**

- process message

**Assert:**

- inbox record not persisted
- transaction rolled back

---

### TC-06

- #### Should_ProcessMessageOnlyOnce_When_DuplicateEventsArrive

**Scenario:**

- Two identical messages arrive sequentially.

**Arrange:**

- send event with same messageId twice

**Act:**

- process both events

**Assert:**

- handler executed once
- second execution skipped

---

### TC-07

- #### Should_UseConsumerName_When_RecordingInboxEntry

**Scenario:**

- Inbox records must include the consumer name to support multiple handlers.

**Arrange:**

- configure `IInboxConsumerNameProvider`

**Act:**

- process message

**Assert:**

- inbox record contains correct consumer identifier

---

### TC-08

- #### Should_PersistInboxRecordAtomically_With_HandlerSideEffects

**Scenario:**

- Inbox record and handler side effects must commit within the same transaction.

**Arrange:**

- handler modifies database state

**Act:**

- process message

**Assert:**

- both changes committed together
- no partial state

---

### TC-09

- #### Should_HandleConcurrentDuplicateMessagesSafely

**Scenario:**

- Two identical messages arrive concurrently.

**Arrange:**

- simulate parallel processing

**Act:**

- process both messages concurrently

**Assert:**

- only one handler execution occurs
- inbox record prevents duplicate execution

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- message identifiers do not expose internal infrastructure details
- inbox records contain only safe metadata
- sensitive payload information is not logged

### 3.2 Observability & Traceability

- `messageId` appears in logs
- `correlationId` flows through the mediator pipeline
- skipped duplicates are logged for debugging

### 3.3 Contract Stability

- message identifier format remains stable
- inbox records remain compatible with future event versions

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - integration events with messageId
  - duplicated events
  - simulated handler failures
  - concurrent message deliveries

Example event:

```text
ProjectCreatedIntegrationEvent
MessageId: 9f3a8c3a-2e7c-41ab-9a7c-01a1c3c3c1ab
Consumer: ProjectsModule
```

- **Expected Outputs:**
  - inbox record created on first processing
  - duplicate events skipped
  - handler executed once
  - atomic persistence of inbox record and side effects

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Write failing tests validating:
   - duplicate detection
   - pipeline behavior
   - inbox persistence rules

2. **GREEN:**  
   Implement:
   - `InboxPipelineBehavior`
   - `EfCoreInboxStore`
   - `InboxMessage` entity

3. **REFACTOR:**  
   Improve:
   - pipeline integration
   - transaction handling
   - performance of existence checks

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass
- [ ] All Integration Tests pass
- [ ] Duplicate message handling verified
- [ ] Transactional consistency verified
- [ ] Documentation updated

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Application.UnitTests/
 │       └── Messaging/
 │           ├── InboxBehaviorTests.cs
 │           ├── InboxPipelineBehaviorTests.cs
 │           └── InboxCorrelationBehaviorTests.cs
 │
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       └── Inbox/
 │           ├── InboxStoreTests.cs
 │           └── InboxPersistenceTests.cs
 │
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── InboxProcessingTests.cs
```

---

## Summary

This test plan validates the **Inbox Pattern implementation that guarantees idempotent event processing**. By recording processed messages and integrating duplicate detection into the mediator pipeline, the system ensures that event handlers execute at most once per message. Combined with the Transactional Outbox pattern, this architecture provides reliable, fault‑tolerant event-driven communication within the modular DDD system.

---
