# Test Plan: ADR-0033 Event Ordering and Delivery Guarantees

## Status

Accepted

## Context

This test plan validates the **Event Ordering and Delivery Guarantees** defined in [ADR-0033](/docs/adr/0033-event-ordering-and-delivery-guarantees.md).

The system uses the Transactional Outbox pattern with an asynchronous outbox processor. This design guarantees durability before publication, but it also implies distributed messaging realities such as duplicate delivery, retries, eventual consistency, and only best-effort ordering.

This plan ensures the architecture and implementation consistently reflect the official guarantees:

- **Delivery Guarantee:** at-least-once
- **Ordering Guarantee:** best-effort ordering within a single outbox stream, typically by `OccurredOn`
- **Consumer Assumption:** duplicates and out-of-order delivery are both possible
- **Non-Guarantee:** exactly-once delivery and strict global ordering are not provided

## Test Strategy

- **Unit Tests:**
  - Validate retry behavior in the outbox processor/publisher.
  - Validate that publish failures do not immediately remove messages from the outbox.
  - Validate that successful publish marks messages as processed.
  - Validate query ordering policy based on `OccurredOn`.
  - Validate poison/failure transition after retry limit.
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Validate persisted outbox messages are retried after transient publish failures.
  - Validate duplicate publish attempts can occur after partial failures.
  - Validate outbox selection is ordered by `OccurredOn`.
  - Validate eventual consistency behavior from command execution to asynchronous publication.
  - Target Project: `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`
  - Potential module flow validation: `tests/IntegrationTests/Hector.Modules.Projects.IntegrationTests`

- **Architecture Tests:**
  - Validate consumers do not depend on exactly-once assumptions.
  - Validate outbox processor uses approved query policy / ordering strategy.
  - Validate Inbox-related contracts exist for idempotent consumption support.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

- **Included:**
  - At-least-once delivery semantics through retryable outbox processing.
  - Durable persistence before publish.
  - Message retry after transient publish failure.
  - Message failure marking after retry exhaustion.
  - Best-effort ordering by `OccurredOn`.
  - Explicit absence of exactly-once guarantees.
  - Duplicate delivery possibility due to retries/partial failures.
  - Eventual consistency expectations.
  - Consumer requirement for idempotency / Inbox usage.

- **Excluded:**
  - Broker-specific guarantees for Kafka, RabbitMQ, Azure Service Bus.
  - Global ordering across modules.
  - Strong partition ordering semantics in future infrastructure.
  - SLA/performance benchmarking of throughput.

---

## 2. Test Cases

### TC-01: Should_PersistOutboxMessageBeforePublish_ForDurabilityGuarantee

**Scenario:** The producer must guarantee durability before asynchronous publishing begins.

**Arrange:**

- Execute an application command that raises a domain event and maps it to an integration event.
- Simulate a state where outbox publishing has not yet executed.

**Act:**

- Persist the unit of work.

**Assert:**

- The corresponding `OutboxMessage` exists in the database after transaction commit.
- The message is stored before any successful broker publish is required.
- This verifies “durability before publish”.

---

### TC-02: Should_RetryPublishing_WhenPublisherFailsTransiently

**Scenario:** The outbox processor must retry publication when a transient failure occurs.

**Arrange:**

- Create an outbox message in pending state.
- Configure `IOutboxPublisher` to fail on the first attempt and succeed on a later attempt.

**Act:**

- Execute outbox processing across multiple cycles.

**Assert:**

- The publisher is invoked more than once for the same message.
- The message is not marked processed after the first failed attempt.
- The message is eventually marked processed after a successful retry.

---

### TC-03: Should_AllowDuplicatePublishAttempts_ForSameOutboxMessage

**Scenario:** Because delivery is at-least-once, the same logical event may be published more than once.

**Arrange:**

- Create an outbox message.
- Simulate partial failure after publish but before the processor can safely persist final processed state.

**Act:**

- Re-run outbox processing.

**Assert:**

- The same message may be published again.
- The architecture treats this as valid behavior under at-least-once delivery.
- No exactly-once assumption is embedded in the processor.

---

### TC-04: Should_MarkMessageAsProcessed_AfterSuccessfulPublish

**Scenario:** Once publication succeeds, the outbox message should be marked as processed to stop further retries.

**Arrange:**

- Create a pending outbox message.
- Configure `IOutboxPublisher` to succeed.

**Act:**

- Run the outbox processor.

**Assert:**

- The message is marked as processed.
- Subsequent processing cycles do not republish the same already-processed message.

---

### TC-05: Should_MarkMessageAsFailed_AfterRetryLimitIsReached

**Scenario:** If publication keeps failing beyond the configured retry limit, the message must be marked as failed/poisoned.

**Arrange:**

- Create a pending outbox message.
- Configure `IOutboxPublisher` to fail repeatedly.
- Set retry limit in `OutboxProcessingPolicy` / options.

**Act:**

- Execute enough processing cycles to exhaust retries.

**Assert:**

- The message transitions to failed state.
- It is no longer retried indefinitely.
- Failure metadata is recorded for diagnosis where supported.

---

### TC-06: Should_SelectPendingOutboxMessagesOrderedByOccurredOn

**Scenario:** Best-effort ordering within a single outbox stream must use `OccurredOn` ordering.

**Arrange:**

- Insert multiple pending outbox messages with deliberately different `OccurredOn` timestamps.

**Act:**

- Run the outbox processor query for the next batch.

**Assert:**

- Messages are selected in ascending `OccurredOn` order.
- Processing attempts follow that order within the same batch when possible.

---

### TC-07: Should_NotGuaranteeStrictOrdering_WhenRetryDelaysEarlierMessage

**Scenario:** A later message may be delivered before an earlier one if the earlier message is delayed by retries.

**Arrange:**

- Create two outbox messages:
  - Message A with earlier `OccurredOn`
  - Message B with later `OccurredOn`
- Configure publishing of A to fail transiently.
- Configure publishing of B to succeed.

**Act:**

- Run processing cycles.

**Assert:**

- Message B may complete publication before A.
- This behavior is accepted and documented.
- Tests verify the system provides only best-effort ordering, not strict ordering.

---

### TC-08: Should_NotGuaranteeGlobalOrdering_AcrossIndependentModules

**Scenario:** Events published by different modules must not be assumed to follow a single globally ordered stream.

**Arrange:**

- Produce outbox messages from two different modules.
- Ensure timestamps overlap or are close enough to create ambiguous sequencing.

**Act:**

- Execute concurrent or independent processing.

**Assert:**

- No architectural rule assumes a globally ordered event stream across modules.
- Ordering guarantees remain local and best-effort only.

---

### TC-09: Should_ExposeEventualConsistency_BetweenCommandCommitAndAsyncPublication

**Scenario:** After command completion, the write transaction may be committed before external publication occurs.

**Arrange:**

- Execute a command that results in an outbox message.
- Delay or pause the outbox processor.

**Act:**

- Observe system state immediately after command success and before processor execution.

**Assert:**

- Business state is committed.
- Integration event is not necessarily published yet.
- This validates eventual consistency expectations.

---

### TC-10: Should_SupportIdempotentConsumption_ViaInboxPatternContracts

**Scenario:** Since duplicate delivery is expected, the architecture must support consumer idempotency through Inbox abstractions.

**Arrange:**

- Inspect application messaging contracts and inbox infrastructure types:
  - `IInboxMessage`
  - `IInboxStore`
  - inbox pipeline behaviors

**Act:**

- Run architecture/unit validations over inbox components.

**Assert:**

- Inbox abstractions exist and are usable for deduplication.
- The platform provides an official path for idempotent consumer design.

---

### TC-11: Should_NotClaimExactlyOnceDelivery_InArchitectureContracts

**Scenario:** The architecture must not expose APIs, names, or tests that imply exactly-once delivery.

**Arrange:**

- Inspect outbox/integration event abstractions and tests.

**Act:**

- Validate contract names, ADR-aligned test expectations, and processor behavior.

**Assert:**

- No implementation contract guarantees exactly-once semantics.
- Retry-based behavior clearly aligns with at-least-once semantics.

---

### TC-12: Should_ProcessBatchInBestEffortOccurredOnOrder

**Scenario:** Within a single processor batch, pending messages should be attempted in `OccurredOn` order whenever possible.

**Arrange:**

- Insert a batch of pending messages with sequential timestamps.

**Act:**

- Process one batch.

**Assert:**

- Publish attempts occur in ascending `OccurredOn` order.
- If one message fails, later messages may still be handled according to the implemented batch policy, but strict completion ordering is not assumed.

---

### TC-13: Should_RetainPendingState_WhenPublishFailsBeforeProcessedFlagIsSaved

**Scenario:** If publication succeeds externally but local processed-state persistence fails, the message may be retried and duplicated.

**Arrange:**

- Simulate:
  - successful call to `IOutboxPublisher`
  - failure while saving processed marker/state

**Act:**

- Re-run outbox processing.

**Assert:**

- The message remains eligible for retry.
- A duplicate publish attempt can occur.
- This confirms why exactly-once is not guaranteed.

---

### TC-14: Should_UseApprovedOutboxQueryOrderingPolicy_ArchitectureGuard

**Scenario:** The outbox processor must use the approved ordering/query policy rather than ad-hoc selection logic.

**Arrange:**

- Inspect processor dependencies and existing architecture tests around selection/query policy.

**Act:**

- Validate that the processor uses the designated query policy / ordering mechanism.

**Assert:**

- Processor adheres to the approved outbox query policy.
- Ordering semantics remain centralized and reviewable.

---

## 3. Non-Functional Validation Points

### 3.1 Reliability

- Messages must survive process restarts because they are persisted before publish.
- Retry behavior must tolerate transient transport failures.
- Failure state must prevent infinite uncontrolled retry loops.

### 3.2 Consistency Expectations

- System behavior must clearly reflect eventual consistency.
- Tests must prove that publication can lag behind committed business state.
- Duplicate and out-of-order delivery must be treated as expected behavior, not exceptional behavior.

### 3.3 Observability

- Retry counts, failure state, and processing timestamps should be diagnosable.
- Processing logs should distinguish:
  - pending
  - processed
  - failed
  - retried
- Ordering-related troubleshooting should be possible using `OccurredOn`.

### 3.4 Stability

- Batch processing should remain deterministic enough for repeatable tests based on `OccurredOn`.
- Tests should avoid assuming single-threaded strict ordering when concurrency is part of the design.

---

## 4. Test Data

- **Inputs:**
  - Pending outbox messages with varying `OccurredOn` timestamps.
  - Publisher behaviors:
    - always succeed
    - fail once then succeed
    - always fail
    - succeed publish but fail processed-state persistence
  - Multiple module-produced events.
  - Configured retry limits and batch sizes.

- **Expected Outputs:**
  - Durable outbox persistence before publish.
  - Multiple publish attempts for the same message in retry scenarios.
  - Processed state after successful publish.
  - Failed state after retry exhaustion.
  - Best-effort ascending selection by `OccurredOn`.
  - Observable eventual consistency between transaction commit and publication.
  - No reliance on exactly-once or strict global ordering.

---

## 5. TDD Execution Plan

1. **RED**
   - Write a failing test proving a transient publish failure leaves the message pending.
   - Write a failing test proving a later message may complete before an earlier retried one.
   - Write a failing test proving duplicate publish attempts are possible after partial failure.

2. **GREEN**
   - Implement/adjust:
     - outbox retry behavior
     - processed/failure state transitions
     - `OccurredOn` ordering in pending message selection
     - retry limit enforcement
   - Ensure tests reflect at-least-once semantics rather than exactly-once assumptions.

3. **REFACTOR**
   - Extract reusable builders for outbox messages with custom:
     - `OccurredOn`
     - retry counts
     - statuses
   - Centralize publisher failure-mode test doubles.
   - Keep ordering and retry assertions explicit and readable.

---

## 6. Exit Criteria

- [ ] Outbox messages are persisted durably before publish.
- [ ] Transient publish failures result in retry, not message loss.
- [ ] Duplicate publish attempts are demonstrably possible.
- [ ] Successful publish marks messages as processed.
- [ ] Retry exhaustion marks messages as failed/poisoned.
- [ ] Pending message selection uses `OccurredOn` ordering.
- [ ] Tests demonstrate that strict ordering can be violated under retry/concurrency conditions.
- [ ] Tests demonstrate eventual consistency between transaction commit and asynchronous publication.
- [ ] Architecture confirms support for idempotent consumers via Inbox abstractions.
- [ ] No implementation or test implies exactly-once delivery.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       ├── OutboxProcessorDeliveryGuaranteeTests.cs
 │       ├── OutboxProcessorOrderingTests.cs
 │       ├── OutboxProcessorRetryStateTransitionTests.cs
 │       └── OutboxConsumerAssumptionTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.BuildingBlocks.Persistence.IntegrationTests/
 │       ├── OutboxAtLeastOnceDeliveryTests.cs
 │       ├── OutboxOrderingBestEffortTests.cs
 │       └── OutboxEventualConsistencyTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.Modules.Projects.IntegrationTests/
 │       └── ProjectEventPublicationConsistencyTests.cs
 │
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         ├── OutboxDeliveryGuaranteeArchitectureTests.cs
         └── OutboxOrderingPolicyArchitectureTests.cs
```

## 8. Implementation Notes

- Existing tests that are likely relevant and should be aligned/extended:
  - `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests/OutboxProcessorPoisonMessageTests.cs`
  - `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests/OutboxProcessingPolicyTests.cs`
  - `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests/OutboxPublisherTests.cs`
  - `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests/OutboxProcessorTests.cs`
  - `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests/OutboxProcessorDeserializationTests.cs`
  - `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests/OutboxTransactionalConsistencyTests.cs`
  - `tests/ArchitectureTests/Hector.ArchitectureTests/OutboxProcessorSelectionRuleTests.cs`
  - `tests/ArchitectureTests/Hector.ArchitectureTests/OutboxQueryPolicyUsageTests.cs`
- Tests must explicitly avoid asserting exactly-once semantics.
- Where duplicate publication is hard to simulate end-to-end, use focused unit tests around processor state transitions and failure windows.
- Ordering tests should assert selection/attempt order, not unrealistic global completion order.
- Consumer-side validation should focus on architectural readiness for idempotency, not on a specific consumer implementation unless one exists.

## Summary

This test plan ensures that [ADR-0033](/docs/adr/0033-event-ordering-and-delivery-guarantees.md) is enforced through automated validation of the system’s actual delivery and ordering semantics. It confirms that the architecture provides at-least-once delivery, best-effort ordering by OccurredOn, and eventual consistency, while explicitly rejecting assumptions of exactly-once delivery or strict global ordering. This protects developers from building workflows on top of false messaging guarantees and aligns the platform with realistic distributed systems behavior.
