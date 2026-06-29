# Test Plan: ADR-0016 Integrate Domain Event Dispatching with EF Core Save Pipeline

## Status

Accepted

## Context

This test plan validates the architectural decision defined in

[ADR‑0016](/docs/adr/0016-integrate-domain-event-dispatching-with-ef-core-save-pipeline.md): Integrate Domain Event Dispatching with EF Core Save Pipeline.

The decision introduces a framework-controlled integration between EF Core persistence and domain event dispatching inside HectorDbContext.

According to [ADR‑0013](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md) and [ADR‑0015](/docs/adr/0015-implement-mediator-based-domain-event-dispatcher.md), domain events raised by aggregates must be collected during the persistence workflow and dispatched only after the database state has been successfully persisted.

[ADR‑0016](/docs/adr/0016-integrate-domain-event-dispatching-with-ef-core-save-pipeline.md) formalizes the integration point by overriding `SaveChangesAsync` in HectorDbContext so that the persistence pipeline automatically:

- collects domain events from tracked aggregates
- persists entity state using EF Core
- dispatches the collected events through `IDomainEventDispatcher`
- clears domain events from aggregate roots after successful dispatch

This behavior is critical because it guarantees:

- consistent domain event dispatch across modules
- strict ordering between persistence and event dispatch
- no manual event publishing in application services
- alignment with the internal mediator-based event pipeline

The test plan validates the correctness, reliability, and consistency of this EF Core save pipeline behavior.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on helper logic such as domain event collection from tracked aggregates if extracted into dedicated methods.

  - **Target Project:** `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Focus on full EF Core save pipeline behavior, ensuring that events are collected, persisted, dispatched, and cleared in the correct order.

  - **Target Project:** `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

### Included

- Domain event collection from EF Core tracked aggregate roots
- Invocation of `IDomainEventDispatcher` after successful `SaveChangesAsync`
- Correct ordering of persistence and dispatch
- Propagation of `CancellationToken`
- Clearing domain events after successful dispatch
- Ensuring domain events are not dispatched if persistence fails
- Ensuring domain events remain on aggregates when persistence fails
- Propagation of dispatch exceptions to the caller

### Excluded

- Integration event publishing
- Transactional outbox behavior
- Background event processing
- Cross-process messaging
- Event retries or durable messaging guarantees

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_DispatchDomainEvents_When_SaveChangesAsyncSucceeds

**Scenario:**

- Domain events raised by tracked aggregate roots must be dispatched when SaveChangesAsync completes successfully.

**Arrange:**

- Create a test aggregate that raises a domain event.
- Attach it to a `HectorDbContext`.
- Register a test implementation of `IDomainEventDispatcher`.

**Act:**

- Call `SaveChangesAsync`.

**Assert:**

- The dispatcher is invoked.
- The domain event raised by the aggregate is passed to the dispatcher.

---

### TC-02

- #### Should_DispatchAllCollectedDomainEvents_When_MultipleAggregatesContainEvents

**Scenario:**

- When multiple aggregates contain domain events, all events must be collected and dispatched.

**Arrange:**

- Create multiple aggregates with domain events.
- Attach them to the DbContext.

**Act:**

- Execute `SaveChangesAsync`.

**Assert:**

- All domain events from all aggregates are passed to `IDomainEventDispatcher`.
- No events are lost.

---

### TC-03

- #### Should_PersistStateBeforeDispatch_When_SavePipelineExecutes

**Scenario:**

- Domain event dispatch must occur only after the database save operation succeeds.

**Arrange:**

- Create an aggregate that raises a domain event.
- Attach to DbContext.

**Act:**

- Execute `SaveChangesAsync`.

**Assert:**

- Database state changes are persisted.
- Dispatcher invocation occurs after persistence.

---

### TC-04

- #### Should_ClearDomainEvents_When_DispatchCompletesSuccessfully

**Scenario:**

- Domain events must be cleared from aggregates after successful dispatch.

**Arrange:**

- Create an aggregate that raises domain events.
- Attach to DbContext.

**Act:**

- Execute `SaveChangesAsync`.

**Assert:**

- Domain events collection on the aggregate is empty after save completes.

---

### TC-05

- #### Should_NotDispatchDomainEvents_When_SaveChangesFails

**Scenario:**

- If the database save operation fails, domain events must not be dispatched.

**Arrange:**

- Create an aggregate with a domain event.
- Configure DbContext to cause persistence failure.

**Act:**

- Execute `SaveChangesAsync`.

**Assert:**

- `IDomainEventDispatcher` is not invoked.

---

### TC-06

- #### Should_RetainDomainEvents_When_PersistenceFails

**Scenario:**

- If persistence fails, domain events must remain stored on the aggregate.

**Arrange:**

- Create aggregate with domain event.
- Cause EF Core persistence failure.

**Act:**

- Call `SaveChangesAsync`.

**Assert:**

- Domain events remain present on the aggregate.

---

### TC-07

- #### Should_PropagateCancellationToken_When_SaveChangesAsyncIsInvoked

**Scenario:**

- Cancellation tokens must propagate from `SaveChangesAsync` to the dispatcher.

**Arrange:**

- Create a cancellation token.
- Configure dispatcher test double to capture token.

**Act:**

- Call `SaveChangesAsync(cancellationToken)`.

**Assert:**

- Dispatcher receives the same cancellation token.

---

### TC-08

- #### Should_PropagateDispatchException_When_DispatchFailsAfterPersistence

**Scenario:**

- If dispatch fails after persistence succeeds, the exception must propagate to the caller.

**Arrange:**

- Configure dispatcher to throw an exception.
- Create aggregate with domain event.

**Act:**

- Call `SaveChangesAsync`.

**Assert:**

- The exception thrown by the dispatcher propagates to the caller.

---

### TC-09

- #### Should_HandleNoDomainEvents_When_NoAggregatesContainEvents

**Scenario:**

- The save pipeline should behave normally when no aggregates contain domain events.

**Arrange:**

- Create entities without domain events.
- Attach them to DbContext.

**Act:**

- Execute `SaveChangesAsync`.

**Assert:**

- Persistence succeeds.
- Dispatcher is not invoked.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that persistence or dispatch exceptions do not leak sensitive infrastructure details such as database connection strings, stack traces, or internal identifiers to external layers.

### 3.2 Observability & Traceability

Verify that domain event dispatch maintains correlation and tracing context when flowing through the mediator pipeline.

### 3.3 Contract Stability

Verify that the contract between `HectorDbContext` and `IDomainEventDispatcher` remains stable and consistent across modules.

---

## 4. Test Data

Define specific sample data used during testing:

- **Inputs:**
  - Aggregates raising domain events
  - Aggregates without domain events
  - Multiple aggregates with events
  - Cancellation tokens
  - Simulated persistence failures
  - Simulated dispatch failures

- **Expected Outputs:**
  - Domain events collected from aggregates
  - Dispatcher invoked with collected events
  - Events cleared after successful dispatch
  - Events retained when persistence fails
  - Exceptions propagated when dispatch fails

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Define failing integration tests validating domain event dispatch during the EF Core save pipeline.

2. **GREEN:**  
   Implement `SaveChangesAsync` override in `HectorDbContext` to collect, dispatch, and clear domain events.
3. **REFACTOR:**  
   Improve domain event collection logic and ensure consistent behavior across module DbContexts.

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
 │       └── DomainEvents/
 │           └── DomainEventCollectionTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── HectorDbContextDomainEventDispatchTests.cs
```

---

## Summary

This test plan validates the integration of domain event dispatching with the EF Core save pipeline implemented by `HectorDbContext`. The tests ensure that domain events raised by aggregates are automatically collected, dispatched only after successful persistence, and cleared afterward. By validating the ordering guarantees and failure behavior of the persistence pipeline, this test plan protects a critical architectural invariant of the system: reliable and consistent in-process domain event propagation across all modules.

---
