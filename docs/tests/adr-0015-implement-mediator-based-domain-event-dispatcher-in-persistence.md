# Test Plan: ADR-0015 Implement Mediator-Based Domain Event Dispatcher in Persistence

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0015](/docs/adr/0015-implement-mediator-based-domain-event-dispatcher.md).

[ADR-0015](/docs/adr/0015-implement-mediator-based-domain-event-dispatcher.md) introduces a mediator-based implementation of `IDomainEventDispatcher` inside `Hector.BuildingBlocks.Persistence`.

The dispatcher acts as the bridge between:

- Domain Events raised by aggregates
- Application-level handlers executed via the internal mediator

The dispatcher is responsible for publishing domain events through the internal `IMediator` introduced in [ADR‑0014](/docs/adr/0014-adopt-internal-mediator-for-CQRS.md).

The dispatcher is invoked by the persistence save pipeline described in [ADR‑0013](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md).

Expected event flow:

```text
Aggregate raises domain event
      ↓
AggregateRoot stores event internally
      ↓
HectorDbContext.SaveChangesAsync()
      ↓
Collect domain events from tracked aggregates
      ↓
Persist database changes
      ↓
Dispatch events through IDomainEventDispatcher
      ↓
Mediator publishes events to handlers
      ↓
Clear domain events from aggregates
```

This mechanism ensures that:

- domain logic remains isolated from infrastructure,
- domain events are dispatched only after successful persistence,
- application handlers can react to domain events through mediator notifications.
This test plan verifies the correctness, reliability, and architectural alignment of the dispatcher.

## Test Strategy

Testing will occur at two levels:

- **Unit Tests:**
  - Focus on the DomainEventDispatcher behavior:
    - publishing events through IMediator
    - correct handling of multiple events
    - cancellation propagation
    - interaction with mediator notifications

  - **Target Project:** `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Focus on the full persistence pipeline:
    - domain event collection
    - dispatcher invocation after save
    - mediator handler execution

  - **Target Project:** `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

### Included

- Dispatching `IDomainEvent` through `IMediator`
- Handling multiple domain events in a single dispatch call
- Cancellation token propagation
- Interaction between persistence pipeline and dispatcher
- Support for multiple handlers per domain event
- Integration between `HectorDbContext` and IDomainEventDispatcher

### Excluded

- External messaging infrastructure
- Integration event publishing
- Transactional outbox processing
- Background workers
- Distributed messaging guarantees
- Event replay or retry mechanisms

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_PublishDomainEventThroughMediator_When_DispatchAsyncIsCalled

**Scenario:**

- When the dispatcher receives a domain event, it must publish the event using `IMediator`.

**Arrange:**

- Create a test domain event implementing `IDomainEvent`
- Mock `IMediator`
- Instantiate `DomainEventDispatcher`

**Act:**

- Call `DispatchAsync([domainEvent])`.

**Assert:**

- `IMediator.PublishAsync` is invoked
- The published notification corresponds to the domain event

---

### TC-02

- #### Should_PublishAllDomainEvents_When_MultipleEventsAreDispatched

**Scenario:**

- The dispatcher must publish every domain event in the provided sequence.

**Arrange:**

- Create multiple domain events
- Mock mediator

**Act:**

- Call `DispatchAsync(events)`.

**Assert:**

- TMediator publish is called once per domain event
- All events are dispatched

---

### TC-03

- #### Should_PropagateCancellationToken_When_DispatchAsyncIsInvoked

**Scenario:**

- Cancellation tokens must flow through the dispatcher into mediator calls.

**Arrange:**

- Create a cancellation token
- Mock mediator

**Act:**

- `DispatchAsync(events, cancellationToken)`

**Assert:**

- The same token is passed to `IMediator.PublishAsync`

---

### TC-04

- #### Should_SupportMultipleHandlers_When_DomainEventIsPublished

**Scenario:**

- A single domain event may have multiple handlers.

**Arrange:**

- Register two mediator handlers for the same domain event
- Register dispatcher and mediator in DI

**Act:**

- Dispatch the domain event.

**Assert:**

- Both handlers are executed
- Execution occurs through mediator notification publishing

---

### TC-05

- #### Should_BeInvokedAfterSuccessfulPersistence_When_SaveChangesCompletes

**Scenario:**

- Domain events must only be dispatched after successful persistence.

**Arrange:**

- Create aggregate that raises domain event
- Attach to `HectorDbContext`
- Register dispatcher

**Act:**

- Call `SaveChangesAsync()`

**Assert:**

- Database changes succeed
- Dispatcher is invoked
- Mediator handlers execute after persistence

---

### TC-06

- #### Should_NotDispatchDomainEvents_When_SaveChangesFails

**Scenario:**

- Domain events must not be dispatched if persistence fails.

**Arrange:**

- Create aggregate with domain event
- Force database failure during save

**Act:**

- Execute `SaveChangesAsync()`

**Assert:**

- Dispatcher is not invoked
- Mediator publish does not occur

---

### TC-07

- #### Should_ClearDomainEvents_When_DispatchCompletesSuccessfully

**Scenario:**

- Domain event buffers must be cleared after successful dispatch.

**Arrange:**

- Aggregate raises domain event
- Save changes successfully

**Act:**

- Invoke save pipeline.

**Assert:**

- Domain events are cleared from aggregate
- Subsequent save does not redispatch the same events

---

### TC-08

- #### Should_HandleEmptyDomainEventCollection_When_NoEventsExist

**Scenario:**

- Dispatcher should behave safely when no domain events are provided.

**Arrange:**

- Create empty event collection.

**Act:**

- `DispatchAsync([])`

**Assert:**

- No mediator publish occurs
- No exception is thrown

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that domain event dispatch does not leak internal persistence details or mediator infrastructure exceptions to higher layers.

### 3.2 Observability & Traceability

Ensure that mediator-based dispatch allows tracing behaviors (logging/correlation) to observe domain event propagation.

### 3.3 Architectural Boundary Validation

Verify that:

- Domain layer has no dependency on mediator
- Persistence references Application messaging abstractions only
- Application handlers react to domain events via mediator

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Domain event instances
  - Aggregates raising events
  - Multiple handlers registered
  - Cancellation tokens
  - Successful and failing persistence scenarios

- **Expected Outputs:**
  - Mediator publish invoked per domain event
  - Handlers executed
  - Domain events cleared after dispatch
  - No dispatch on persistence failure

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Create failing tests for:
   - mediator publishing
   - multi-event dispatch
   - cancellation propagation
   - integration with persistence pipeline
2. **GREEN:**  
Implement minimal `DomainEventDispatcher` using `IMediator`.
3. **REFACTOR:**  
Improve error handling, event sequencing, and separation between dispatcher and persistence pipeline.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass
- [ ] All Integration Tests pass
- [ ] Domain events are dispatched only after successful persistence
- [ ] Mediator integration verified
- [ ] No dependency from Domain layer to mediator infrastructure

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       ├── DomainEventDispatcherTests.cs
 │       └── DomainEventMediatorIntegrationTests.cs
 │
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         ├── DomainEventDispatchPipelineTests.cs
         └── DomainEventPersistenceFlowTests.cs
```

---

## Summary

This ADR establishes a mediator-based implementation of `IDomainEventDispatcher` within the persistence layer to bridge Domain Events and the internal application messaging pipeline.

The dispatcher leverages the internal `IMediator` abstraction (introduced in [ADR‑0014](/docs/adr/0014-adopt-internal-mediator-for-CQRS.md)) to publish domain events as in-process notifications after successful database persistence, as defined by [ADR‑0013](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md).

This decision ensures that:

- domain events remain fully encapsulated within aggregates,
- dispatch occurs only after successful transaction completion,
- application-level handlers are executed through a controlled mediator pipeline,
- no external mediator library dependency is introduced,
- architectural boundaries between Domain, Application, and Persistence remain intact.

The result is a cohesive, framework-controlled mechanism that connects domain event generation to application-level reactions while preserving modular monolith constraints and DDD principles.

---
