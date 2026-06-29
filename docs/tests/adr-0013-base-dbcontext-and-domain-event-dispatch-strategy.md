# Test Plan: ADR-0013 Base DbContext and Domain Event Dispatch Strategy

## Status

Accepted — Domain event dispatching strategy superseded by [ADR-0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md)

## Context

This test plan validates the architectural decision defined in [ADR-0013](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md).

[ADR-0013](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md) introduced a shared persistence base class, `HectorDbContext`, to standardize persistence behavior across all module DbContexts in the modular monolith architecture.

The original goal of this ADR was to provide a consistent EF Core-based Unit of Work implementation and a unified save pipeline for:

- aggregate persistence,
- domain event collection,
- persistence conventions,
- and event dispatch behavior.

As the architecture evolved, the original assumption of direct in-memory domain event dispatch from the EF Core save pipeline was superseded by [ADR-0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md), which established the Transactional Outbox pattern as the authoritative strategy for reliable event delivery.

Therefore, this test plan validates [ADR‑0013](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md) in its current architectural meaning, not in its historical form.

The focus of this plan is to verify that:

- all module DbContexts inherit from `HectorDbContext`,
- shared persistence conventions are consistently applied,
- domain events are collected during persistence,
- outbox messages are created transactionally with aggregate state changes,
- domain events are cleared only after successful persistence,
- and no test incorrectly enforces the superseded direct dispatch behavior as the active contract.

This is critical for architectural consistency, transaction safety, persistence reliability, and keeping domain logic free from infrastructure concerns.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on `HectorDbContext` save pipeline behavior, domain event collection, outbox message creation, and domain event clearing rules.

  - **Target Project:** `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Focus on full EF Core persistence behavior, transactional consistency between aggregate state and outbox records, and module DbContext inheritance/use of shared persistence conventions.

  - **Target Project:** `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

### Included

- Verification that `HectorDbContext` exists as the shared persistence base class
- Verification that module-specific DbContexts inherit from `HectorDbContext`
- Validation of shared save-pipeline behavior across module DbContexts
- Collection of domain events from tracked aggregates during persistence
- Transactional creation of outbox messages from collected domain events
- Clearing domain events only after successful persistence
- Application of shared persistence conventions such as strongly typed ID mapping
- Verification that current persistence behavior aligns with [ADR‑0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md) rather than direct in-memory dispatch

### Excluded

- Direct in-memory domain event publication as the authoritative persistence behavior
- Background outbox processing and message publication
- Outbox cleanup and retry policies
- Distributed event delivery guarantees
- HTTP/API-level behaviors
- Full mediator behavior beyond what is required to validate superseded assumptions

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_InheritFromHectorDbContext_When_ModuleDbContextIsDefined

**Scenario:**

- All module DbContexts must inherit from `HectorDbContext` to ensure standardized persistence behavior across modules.

**Arrange:**

- Identify representative module DbContext implementations such as `ProjectsDbContext`.

**Act:**

- Inspect the DbContext inheritance hierarchy.

**Assert:**

- The module DbContext inherits from `HectorDbContext`.
- No module DbContext bypasses the shared persistence base.

---

### TC-02

- #### Should_CollectDomainEvents_When_SaveChangesAsyncIsCalledForTrackedAggregates

**Scenario:**

- When tracked aggregates contain domain events, the save pipeline should collect those events during persistence.

**Arrange:**

- Create an aggregate that raises one or more domain events.
- Attach the aggregate to a `HectorDbContext`-based context.

**Act:**

- Call `SaveChangesAsync`.

**Assert:**

- Domain events are detected from tracked aggregates.
- The persistence pipeline processes the collected domain events.

---

### TC-03

- #### Should_CreateOutboxMessages_When_DomainEventsAreCollectedDuringPersistence

**Scenario:**

- Collected domain events must be transformed into outbox messages as part of persistence behavior.

**Arrange:**

- Create an aggregate with domain events.
- Configure the context with the required outbox serializer.

**Act:**

- Persist changes through `SaveChangesAsync`.

**Assert:**

- Outbox message records are created for the collected domain events.
- Outbox message payloads correspond to the domain events raised by the aggregate.

---

### TC-04

- #### Should_PersistAggregateChangesAndOutboxMessagesInSameTransaction_When_SaveChangesSucceeds

**Scenario:**

- Aggregate state changes and outbox records must be persisted atomically in the same transaction.

**Arrange:**

- Create an aggregate with domain events.
- Use a real persistence-backed integration test context.

**Act:**

- Call `SaveChangesAsync`.

**Assert:**

- Aggregate state is persisted successfully.
- Corresponding outbox messages are also persisted.
- Both persistence outcomes succeed together as one transactional unit.

---

### TC-05

- #### Should_NotClearDomainEvents_When_SaveChangesFails

**Scenario:**

- If persistence fails, domain events must remain on the aggregate so they are not lost.

**Arrange:**

- Create an aggregate with domain events.
- Force persistence failure during `SaveChangesAsync`.

**Act:**

- Execute `SaveChangesAsync` and capture the failure.

**Assert:**

- Persistence fails.
- Domain events are not cleared from the aggregate.

---

### TC-06

- #### Should_ClearDomainEvents_When_SaveChangesSucceeds

**Scenario:**

- Domain events must be cleared from aggregates only after successful persistence.

**Arrange:**

- Create an aggregate that raises domain events.
- Attach it to a valid `HectorDbContext` instance.

**Act:**

- Call `SaveChangesAsync`.

**Assert:**

- Persistence succeeds.
- Domain events are cleared from the aggregate after successful save.

---

### TC-07

- #### Should_ApplyStronglyTypedIdPersistenceConventions_When_ModuleDbContextUsesHectorDbContext

**Scenario:**

- The shared base DbContext must apply common persistence conventions such as strongly typed ID mapping.

**Arrange:**

- Create an entity using a strongly typed ID.
- Use a module DbContext that derives from `HectorDbContext`.

**Act:**

- Persist and reload the entity.

**Assert:**

- The strongly typed ID is stored and rehydrated correctly.
- No manual converter configuration is required beyond the shared convention mechanism.

---

### TC-08

- #### Should_NotRequireDirectDomainEventDispatcher_When_CurrentPersistenceStrategyUsesOutbox

**Scenario:**

- Although [ADR‑0013](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md) originally assumed direct domain event dispatch from the save pipeline, the current architecture must not require that behavior as the governing persistence strategy.

**Arrange:**

- Inspect `HectorDbContext` persistence behavior and required dependencies.

**Act:**

- Evaluate whether persistence depends on direct `IDomainEventDispatcher` invocation.

**Assert:**

- Persistence behavior is based on outbox persistence rather than direct in-memory dispatch.
- No test treats direct dispatcher invocation as a mandatory success criterion for `HectorDbContext`.

---

### TC-09

- #### Should_RemainConsistentAcrossModules_When_MultipleDbContextsUseSharedBaseContext

**Scenario:**

- All module DbContexts should receive the same standardized save-pipeline behavior from the shared base context.

**Arrange:**

- Identify multiple module DbContexts derived from `HectorDbContext`.

**Act:**

- Compare their persistence behavior and shared conventions.

**Assert:**

- Save behavior is consistent across modules.
- Shared persistence rules are not duplicated inconsistently per module.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that persistence failures do not leak sensitive infrastructure details, internal serializer configuration, or raw transaction internals to upper layers.

### 3.2 Observability & Traceability

Verify that persistence operations involving aggregate saves and outbox creation remain diagnosable and traceable, especially around transaction boundaries and failure scenarios.

### 3.3 Contract Stability

Verify that HectorDbContext remains the stable persistence base contract for module DbContexts, and that the save pipeline continues to align with [ADR‑0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md) rather than reintroducing superseded direct dispatch assumptions.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Aggregate root with one or more domain events
  - Module DbContext such as `ProjectsDbContext`
  - Strongly typed ID entities such as `Project` / `ProjectId`
  - Valid persistence scenarios
  - Forced persistence failure scenarios

- **Expected Outputs:**
  - Domain events collected during save
  - Outbox messages created transactionally
  - Domain events cleared only after successful persistence
  - Shared base DbContext used consistently by module contexts
  - No mandatory dependence on direct in-memory dispatch behavior

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Define failing tests for shared DbContext inheritance, domain event collection, transactional outbox persistence, and domain event clearing rules.
2. **GREEN:** Implement the minimal `HectorDbContext` save pipeline and outbox persistence behavior required by the accepted architecture.
3. **REFACTOR:** Remove superseded direct dispatch assumptions, consolidate shared persistence behavior, and keep module DbContexts thin and consistent.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Security/Non-functional points verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       ├── HectorDbContextTests.cs
 │       └── DomainEventDispatcherTests.cs
 └── IntegrationTests/
     ├── Hector.BuildingBlocks.Persistence.IntegrationTests/
     │   ├── HectorDbContextTests.cs
     │   └── OutboxTransactionalConsistencyTests.cs
     └── Hector.Modules.Projects.IntegrationTests/
         └── CreateProjectTests.cs
```

---

## Summary

This test plan validates [ADR‑0013](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md) in its correct architectural context: as the decision that standardizes the shared base `HectorDbContext` and persistence save pipeline, while recognizing that its original direct domain event dispatch strategy has been superseded by [ADR‑0021](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md). The result is a persistence architecture that remains consistent across modules, protects transaction boundaries, and supports reliable domain event handling through transactional outbox persistence.

---
