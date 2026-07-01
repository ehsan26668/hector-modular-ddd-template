# Test Plan: ADR‑0024 Adopt Distributed Locking for Outbox Processor

## Status

Accepted

## Context

This test plan validates [ADR‑0024](/docs/adr/0024-adopt-distributed-locking-for-outbox-processor.md): *Adopt Distributed Locking for Outbox Processor*.

[ADR‑0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md) introduced the Transactional Outbox pattern, and [ADR‑0022](/docs/adr/0022-outbox-background-processor.md) introduced the background processor that polls and publishes pending outbox messages. In a multi-instance deployment, multiple processors may attempt to process the same pending messages concurrently. Without coordination, this leads to duplicate claims, duplicate publication attempts, and weakened delivery guarantees.

[ADR‑0024](/docs/adr/0024-adopt-distributed-locking-for-outbox-processor.md) addresses this by introducing a database-backed lease-based distributed locking mechanism using `LockId` and `LockedUntil` on outbox messages.

This test plan validates that:

- only eligible messages are claimable
- claims are atomic and ownership-safe
- a processor only loads messages it successfully claimed
- locks are released after success or failure
- expired locks can be reclaimed
- retry metadata is updated correctly
- concurrent processors do not simultaneously own the same message

This ADR is critical because it protects the correctness of outbox processing in horizontally scaled deployments while preserving the architecture’s EF Core-based, provider-agnostic design.

## Test Strategy

### Unit Tests

#### Focus on isolated locking logic, eligibility rules, lease handling, retry state transitions, and ownership filtering

Target project:

- tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests

### Integration Tests

#### Focus on concurrent processors, database-backed conditional claims, lock expiration, and end-to-end outbox processing correctness

Target project:

- tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests

---

## 1. Scope

List exactly what is included and excluded from this test plan to set clear boundaries for the validation process.

### Included

- Selection of eligible outbox messages for claiming
- Filtering rules:
  - `ProcessedOn == null`
  - `RetryCount < MaxRetryCount`
  - `LockedUntil == null || LockedUntil < UtcNow`
- Atomic claim/update behavior
- Assignment of LockId and LockedUntil
- Loading messages by current LockId
- Lock release after successful publish
- Lock release after failed publish
- Retry metadata updates on failure
- Reclaiming messages after lease expiration
- Concurrency safety with multiple processor instances
- Ordering within claimed batch

### Excluded

- External distributed lock providers
- Vendor-specific DB locking primitives
- Poison-message handling beyond retry limit behavior
- Cleanup/retention behavior from [ADR‑0025](/docs/adr/0025-outbox-cleanup-and-retention-policy.md)
- Consumer-side idempotency beyond inbox integration assumptions

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_SelectOnlyEligibleMessages_When_ClaimingBatch

**Scenario:**

- The processor must only attempt to claim messages that are unprocessed, below retry limit, and unlocked or expired.

**Arrange:**

- Create outbox messages with mixed states:
  - unprocessed and unlocked
  - processed
  - retry exhausted
  - currently locked
  - expired lock

**Act:**

- Execute claim selection logic

**Assert:**

- Only eligible message identifiers are selected for claim

---

### TC-02

- #### Should_AssignLockIdAndLockedUntil_When_ClaimSucceeds

**Scenario:**

- When claiming eligible messages, the processor must assign its ownership metadata.

**Arrange:**

- Create eligible outbox messages
- Generate processor `LockId`

**Act:**

- Execute claim operation

**Assert:**

- Claimed rows contain:
  - current `LockId`
  - non-null `LockedUntil`

---

### TC-03

- #### Should_LoadOnlyClaimedMessages_When_ProcessingBatch

**Scenario:**

- After claiming, the processor must load only rows owned by the current processor instance.

**Arrange:**

- Insert messages claimed by different `LockId` values

**Act:**

- Load claimed batch for current processor

**Assert:**

- Only messages with matching `LockId` are loaded

---

### TC-04

- #### Should_NotClaimMessage_When_AlreadyLockedByAnotherProcessor

**Scenario:**

- A message with a non-expired lock must not be claimed by another processor.

**Arrange:**

- Create message with active `LockId` and future `LockedUntil`

**Act:**

- Attempt claim from another processor

**Assert:**

- Claim count is zero
- Ownership remains unchanged

---

### TC-05

- #### Should_ReclaimMessage_When_LockHasExpired

**Scenario:**

- If a lease expires, another processor may reclaim the message.

**Arrange:**

- Create message with expired `LockedUntil`

**Act:**

- Attempt claim from a new processor

**Assert:**

- Message is claimed by new `LockId`
- `LockedUntil` is renewed

---

### TC-06

- #### Should_ReleaseLockAndMarkProcessed_When_PublicationSucceeds

**Scenario:**

- After successful publish, the processor must mark the message processed and release ownership.

**Arrange:**

- Create claimed message
- Mock publisher success

**Act:**

- Process claimed message

**Assert:**

- `ProcessedOn` is set
- `LastAttemptedOn` is set
- `Error` is cleared
- `LockId` is null
- `LockedUntil` is null

---

### TC-07

- #### Should_ReleaseLockAndIncrementRetryCount_When_PublicationFails

**Scenario:**

- After failed publish, the processor must release the lock and record retry metadata.

**Arrange:**

- Create claimed message
- Mock publisher failure

**Act:**

- Process claimed message

**Assert:**

- `RetryCount` incremented
- `LastAttemptedOn` set
- `Error` stored
- `LockId` cleared
- `LockedUntil` cleared
- `ProcessedOn` remains null

---

### TC-08

- #### Should_NotProcessRetryExhaustedMessage_When_RetryLimitReached

**Scenario:**

- Messages that reached the retry limit must not be selected for claim.

**Arrange:**

- Create message with `RetryCount == MaxRetryCount`

**Act:**

- Run claim selection

**Assert:**

- Message is excluded from claim set

---

### TC-09

- #### Should_PreserveOccurrenceOrder_WithinClaimedBatch

**Scenario:**

- Claimed messages must be published in occurrence order within the batch.

**Arrange:**

- Create multiple eligible messages with ordered occurrence timestamps

**Act:**

- Claim and process batch

**Assert:**

- Publisher invoked in expected order

---

### TC-10

- #### Should_PreventSimultaneousOwnership_When_TwoProcessorsClaimSameBatch

**Scenario:**

- Two processors concurrently attempt to claim the same eligible messages.

**Arrange:**

- Seed shared pending outbox messages
- Start two claim attempts concurrently

**Act:**

- Execute both claim operations

**Assert:**

- Each message is owned by at most one processor
- No message ends up with overlapping active ownership

---

### TC-11

- #### Should_ProcessOnlySuccessfullyClaimedMessages_When_ConcurrentClaimsOccur

**Scenario:**

- Even if both processors target the same candidates, each must process only its successfully claimed subset.

**Arrange:**

- Shared pending message set
- Two concurrent processors with different `LockIds`

**Act:**

- Claim and load messages

**Assert:**

- Each processor loads only rows with its own `LockId`
- No processor processes another processor’s claims

---

### TC-12

- #### Should_ClearStaleErrorOnSuccessfulRetry_When_PreviousAttemptFailed

**Scenario:**

- A message that previously failed and is later processed successfully must clear old error state.

**Arrange:**

- Create retriable message with previous `Error` and `RetryCount > 0`

**Act:**

- Process successfully on retry

**Assert:**

- `ProcessedOn` set
- `Error` cleared
- lock released

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that:

- lock metadata does not leak sensitive infrastructure details
- error persistence does not expose stack traces beyond intended storage policy
- logs do not expose connection data or internal secrets

### 3.2 Observability & Traceability

Verify that:

- logs include LockId, message identifier, and event type where relevant
- claim counts and processing outcomes are observable
- retry attempts and lock expiration behavior are traceable in structured logs

### 3.3 Contract Stability

Verify that:

- outbox record schema changes remain backward compatible for locking fields
- processing contract remains stable for future alternative publishers or processors

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - pending outbox messages
  - processed outbox messages
  - locked messages with active leases
  - locked messages with expired leases
  - retry-exhausted messages
  - publisher success/failure scenarios
  - concurrent processor instances with distinct `LockId`s

- **Expected Outputs:**
  - only eligible rows claimed
  - lock fields assigned on successful claim
  - successful messages marked processed and unlocked
  - failed messages retried and unlocked
  - expired locks reclaimed safely
  - no simultaneous ownership for the same row

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Define failing tests for eligibility filtering, atomic claims, lease expiration, and concurrent claim safety.

2. **GREEN:**  
   Implement conditional claim/update logic, ownership-based loading, and lock release behavior.

3. **REFACTOR:**  
   Improve clarity of locking policy, batch orchestration, and logging while preserving behavior and test coverage.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Concurrency safety verified under competing processors.
- [ ] Retry and lock release behavior verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       └── Outbox/
 │           ├── OutboxProcessingPolicyTests.cs
 │           ├── OutboxProcessorLockingTests.cs
 │           ├── OutboxProcessorRetryTests.cs
 │           └── OutboxPublisherTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         ├── OutboxProcessorTests.cs
         ├── OutboxConcurrentProcessingTests.cs
         ├── OutboxLockExpirationTests.cs
         └── OutboxRetryLockReleaseTests.cs
```

---

## Summary

This test plan validates the distributed locking strategy that protects the Outbox Processor in multi-instance deployments. By verifying lease-based claiming, safe ownership boundaries, expiration-based reclamation, and correct release behavior on success and failure, the system can scale horizontally without allowing concurrent processors to simultaneously own the same outbox message. This preserves reliable at-least-once delivery semantics while aligning with the existing EF Core and Modular DDD architecture.

---
