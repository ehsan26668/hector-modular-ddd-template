# Test Plan: ADR-0021 Adopt Transactional Outbox for Domain Event Publishing

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR‑0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md): *Adopt Transactional Outbox for Domain Event Publishing*.

Previously, domain events were dispatched immediately after `SaveChangesAsync` through the `IDomainEventDispatcher`. While suitable for in-process communication, this approach introduced a reliability gap: if event dispatch failed after a successful database commit, domain changes would persist while the event publication could be lost.

The Transactional Outbox pattern addresses this issue by storing domain events in an Outbox table within the same database transaction as aggregate persistence. A background process later reads and publishes these messages asynchronously.

This test plan ensures that:

- domain events are captured and persisted as outbox messages
- outbox records are written within the same transaction as aggregate changes
- events are not lost even if publishing fails
- domain events are cleared only after a successful transaction
- outbox records can be safely processed later by a background dispatcher

## Test Strategy

### Unit Tests

#### Focus on the logic responsible for

- creating outbox messages
- serializing domain events
- persisting outbox entries alongside aggregates

Target project:

- tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests

### Integration Tests

#### Validate transactional guarantees and persistence behavior of the Outbox mechanism using EF Core

Target project:

- tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests

---

## 1. Scope

List exactly what is included and excluded from this test plan to set clear boundaries for the validation process.

### Included

- Extraction of domain events from aggregates
- Creation of outbox messages from domain events
- Serialization of domain event payloads
- Persistence of outbox records within the same transaction as aggregates
- Clearing domain events after successful save
- Deferred publication via background processing
- Outbox message state transitions (Pending → Processed)

### Excluded

- Actual external message broker integration
- Distributed messaging infrastructure
- Event consumer logic
- Idempotency handling on the consumer side (covered by Inbox ADRs)

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_CreateOutboxMessage_When_AggregateRaisesDomainEvent

**Scenario:**

- When an aggregate raises a domain event, an Outbox message must be created during the save pipeline.

**Arrange:**

- Create an aggregate that raises a domain event.
- Attach it to the DbContext.

**Act:**

- Call `SaveChangesAsync`.

**Assert:**

- A corresponding Outbox message exists in the Outbox table.
- The payload contains the serialized domain event.

---

### TC-02

- #### Should_PersistOutboxMessageInSameTransaction_When_SavingAggregate

**Scenario:**

- Aggregate persistence and outbox message creation must occur within the same database transaction.

**Arrange:**

- Create an aggregate that raises a domain event.

**Act:**

- Execute `SaveChangesAsync`.

**Assert:**

- Both aggregate data and Outbox message are persisted.
- If the transaction fails, neither record is stored.

---

### TC-03

- #### Should_ClearDomainEvents_When_SaveChangesSucceeds

**Scenario:**

- Domain events must be cleared from aggregates after a successful transaction.

**Arrange:**

- Create an aggregate that records domain events.

**Act:**

- Persist the aggregate via DbContext.

**Assert:**

- Aggregate no longer contains pending domain events.

---

### TC-04

- #### Should_NotClearDomainEvents_When_SaveChangesFails

**Scenario:**

- If the database transaction fails, domain events must remain attached to the aggregate.

**Arrange:**

- Create an aggregate with a domain event.
- Force a persistence failure.

**Act:**

- Execute `SaveChangesAsync`.

**Assert:**

- Domain events remain on the aggregate.

---

### TC-05

- #### Should_SerializeDomainEventPayload_When_CreatingOutboxRecord

**Scenario:**

- Domain events must be serialized correctly before being stored in the Outbox table.

**Arrange:**

- Create a domain event containing data fields.

**Act:**

- Persist the aggregate.

**Assert:**

- Outbox record payload contains serialized event data.
- Deserialization produces the original event structure.

---

### TC-06

- #### Should_MarkOutboxMessageAsProcessed_When_PublicationSucceeds

**Scenario:**

- After successful event publication, the outbox record must be marked as processed.

**Arrange:**

- Insert a pending Outbox message.

**Act:**

- Execute the Outbox processor.

**Assert:**

- Outbox record status becomes Processed.
- Processing timestamp is recorded.

---

### TC-07

- #### Should_RetryOutboxMessage_When_PublicationFails

**Scenario:**

- If publication fails, the Outbox message must remain available for retry.

**Arrange:**

- Create a pending Outbox message.
- Simulate a publisher failure.

**Act:**

- Execute the Outbox processor.

**Assert:**

- Message remains unprocessed.
- It can be retried in subsequent processing cycles.

---

### TC-08

- #### Should_ProcessMultipleOutboxMessages_When_BackgroundDispatcherRuns

**Scenario:**

- The Outbox processor must handle multiple messages in a batch.

**Arrange:**

- Insert multiple pending Outbox records.

**Act:**

- Execute the Outbox background processor.

**Assert:**

- All messages are processed in sequence.
- Each message is published and marked processed.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Ensure serialized event payloads do not expose sensitive internal state or infrastructure details.

### 3.2 Observability & Traceability

Verify that:

- Outbox processing logs publication attempts
- Correlation IDs propagate from domain events into outbox records
- failures are observable through structured logging

### 3.3 Contract Stability

- Ensure serialized event formats remain compatible with future versions of integration events.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Aggregates raising domain events
  - Domain event payload data
  - Outbox message records
  - Simulated publishing outcomes

- **Expected Outputs:**
  - Persisted Outbox records
  - Serialized domain event payloads
  - Processed message flags
  - Retryable messages on failure

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Write failing tests verifying outbox message creation and transactional consistency.

2. **GREEN:**  
   Implement outbox message persistence within the EF Core save pipeline.

3. **REFACTOR:**  
   Improve serialization logic and background processor behavior while preserving transactional guarantees.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass
- [ ] All Integration Tests pass
- [ ] Transactional consistency between aggregates and outbox messages verified
- [ ] Serialization/deserialization compatibility confirmed
- [ ] Documentation updated

---

## 7. Proposed Test File Layout

```text
tests
 ├─ UnitTests
 │   └─ Hector.BuildingBlocks.Persistence.UnitTests
 │       ├─ OutboxMessageFactoryTests.cs
 │       ├─ OutboxEventSerializationTests.cs
 │       ├─ OutboxPublisherTests.cs
 │       └─ OutboxProcessorTests.cs
 │
 └─ IntegrationTests
     └─ Hector.BuildingBlocks.Persistence.IntegrationTests
         ├─ OutboxTransactionalConsistencyTests.cs
         ├─ OutboxProcessorTests.cs
         └─ OutboxProcessorDeserializationTests.cs
```

---

## Summary

This test plan validates the Transactional Outbox mechanism responsible for durable domain event publication. By ensuring that domain events are stored in the Outbox table within the same transaction as aggregate persistence, the architecture guarantees reliable event delivery, supports retries, and decouples event publication from the request lifecycle.

---
