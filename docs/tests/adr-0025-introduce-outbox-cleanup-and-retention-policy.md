# Test Plan: ADR-0025 Introduce Outbox Cleanup and Retention Policy

## Status

Accepted

## Context

This test plan validates [ADR-0025](/docs/adr/0025-outbox-cleanup-and-retention-policy.md): *Introduce Outbox Cleanup and Retention Policy*.

While [ADR-0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md) and [ADR-0022](/docs/adr/0022-outbox-background-processor.md) ensure reliable event production and processing, they lead to continuous data accumulation in the OutboxMessages table. [ADR-0025](/docs/adr/0025-outbox-cleanup-and-retention-policy.md) introduces a mechanism to prune processed records based on a configurable **Retention Period**, ensuring database performance remains optimal while keeping recent history for diagnostics.

This test plan validates that:

- Only processed messages are deleted.
- The retention period is strictly respected.
- Deletion occurs in batches to prevent transaction log bloat.
- The background job executes reliably according to the configured interval.
- Configuration is validated (Fail-fast).

## Test Strategy

### Unit Tests

#### Focus on the cleanup logic, age calculation, and configuration validation

Target project:

- tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests

### Integration Tests

#### Focus on actual database deletion, batching behavior, and background service execution

Target project:

- tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests

---

## 1. Scope

List exactly what is included and excluded from this test plan to set clear boundaries for the validation process.

### Included

- `OutboxCleaner` logic.
- `OutboxCleanupBackgroundService` execution.
- Retention period threshold calculations (`ProcessedOn < Now - RetentionPeriod`).
- Batched deletion mechanism.
- Configuration validation (`RetentionPeriod`, `CleanupBatchSize`).
- Prevention of deleting unprocessed or failed messages.

### Excluded

- Database-level partitioning (if any).
- Archiving messages to cold storage (out of scope for this ADR).
- Cleanup of Inbox messages (covered by ADR-0023/ADR-0039).

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_OnlyDeleteProcessedMessages_When_CleanupRuns

**Scenario:**

- Ensure that messages with `ProcessedOn == null` are never deleted, regardless of age.

**Arrange:**

- Seed 5 old unprocessed messages.
- Seed 5 old processed messages.

**Act:**

- Execute `CleanupAsync()`.

**Assert:**

- 5 processed messages deleted.
- 5 unprocessed messages remain in the table.

---

### TC-02

- #### Should_RespectRetentionPeriod_When_SelectingMessagesForCleanup

**Scenario:**

- Messages within the retention window must be preserved.

**Arrange:**

- Set `RetentionPeriod` to 7 days.
- Seed a message processed 2 days ago.
- Seed a message processed 10 days ago.

**Act:**

- Execute cleanup.

**Assert:**

- Message from 2 days ago remains.
- Message from 10 days ago is deleted.

---

### TC-03

- #### Should_DeleteInBatches_When_CountExceedsBatchSize

**Scenario:**

- Validate that deletion is constrained by `CleanupBatchSize` to protect database performance.

**Arrange:**

- Set `CleanupBatchSize` to 10.
- Seed 25 eligible messages for deletion.

**Act:**

- Execute one cleanup cycle.

**Assert:**

- Exactly 10 messages are deleted.
- 15 messages remain for the next cycle.

---

### TC-04

- #### Should_ThrowException_When_ConfigurationIsInvalid

**Scenario:**

- Ensure the system fails fast if cleanup parameters are illogical.

**Arrange:**

- Set `RetentionPeriod` to `TimeSpan.Zero` or negative.
- OR set `CleanupBatchSize` to 0.

**Act:**

- Initialize `OutboxCleaner` or `OutboxOptions`.

**Assert:**

- Throws `ValidationException` or `ArgumentOutOfRangeException`.

---

### TC-05

- #### Should_UpdateLastAttemptedOn_EvenIfFailed (Optional/Resiliency)

**Scenario:**

- If the cleanup job fails partway, it shouldn’t crash the background service.

**Arrange:**

- Simulate database timeout during delete.

**Act:**

- `OutboxCleanupBackgroundService` runs.

**Assert:**

- Service logs error.
- Service waits for the next interval and retries.

---

### TC-06

- #### Should_RunPeriodically_AccordingToConfiguration

**Scenario:**

- Validate the background service triggers the cleaner at the specified interval.

**Arrange:**

- Set interval to 1 second (for testing).
- Mock `IOutboxCleaner`.

**Act:**

- Start `OutboxCleanupBackgroundService`.
- Wait for 3 seconds.

**Assert:**

- `Cleaner.CleanupAsync()` invoked multiple times.

---

## 3. Non-Functional Validation Points

### 3.1 Performance

- Verify that the cleanup query uses an index on `ProcessedOn` to avoid full table scans.
- Monitor execution time for large batches to ensure it doesn’t block the `OutboxProcessor`.

### 3.2 Reliability

- Ensure that an exception in the cleanup job does not stop the `OutboxProcessor` (separation of concerns).

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - `OutboxOptions`: { RetentionPeriod: “7.00:00:00”, CleanupBatchSize: 100 }.
  - Mix of messages: (New/Old) x (Processed/Unprocessed/Failed).

- **Expected Outputs:**
  - Clean table containing only:
    - All unprocessed messages.
    - All failed messages within retry limits.
    - Processed messages newer than 7 days.

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Write integration tests that seed old records and expect them to be gone after calling the cleaner.

2. **GREEN:**  
   Implement `OutboxCleaner` using EF Core `ExecuteDeleteAsync` (for EF 7+) or batched LINQ deletes.

3. **REFACTOR:**  
   Ensure the background service is properly registered in DI and shares the same `OutboxOptions`.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] Unit tests for age calculation pass.
- [ ] Integration tests for batched deletion pass.
- [ ] Background service execution verified.
- [ ] Configuration validation logic verified.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       └── Outbox/
 │           ├── OutboxCleanupConfigurationTests.cs
 │           └── OutboxCleanerLogicTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── OutboxCleanupTests.cs
```

---

## Summary

[ADR-0025](/docs/adr/0025-outbox-cleanup-and-retention-policy.md) ensures the long-term health of the persistence layer. By automating the removal of historical outbox data, we maintain low latency for polling queries and prevent storage exhaustion. This test plan ensures that this “garbage collection” process is safe, efficient, and configurable.

---
