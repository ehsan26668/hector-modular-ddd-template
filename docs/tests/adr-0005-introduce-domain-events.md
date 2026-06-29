# Test Plan: ADR-0005 Introduce Domain Events

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0005](/docs/adr/0005-domain-events.md), which introduces Domain Events as a first-class concept in the domain layer.

This validation is critical because Domain Events are the primary mechanism for expressing significant business occurrences while keeping aggregates focused on domain rules instead of side effects or orchestration concerns.

The goal of this plan is to ensure that aggregates can record domain events consistently, that recorded events remain part of the domain model rather than infrastructure concerns, and that the contract for collecting and exposing events is stable, predictable, and extensible.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:** Focus on isolated domain event contracts, aggregate event collection behavior, and event lifecycle within the domain model.

- **Target Project:**
  - `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

- **Integration Tests:** Focus on end-to-end event collection and dispatch-related behavior through persistence or application boundaries where domain events are later consumed.

- **Target Project:**
  - `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

### Included

- Marker contract for domain events
- Base class behavior for domain events
- Aggregate support for collecting domain events
- Recording domain events during aggregate state changes
- Exposing recorded domain events for later dispatch
- Clearing domain events after they are consumed

### Excluded

- Integration event publishing
- Outbox processing
- Cross-module event delivery
- Background processing and retries
- Event bus infrastructure concerns

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_RecordDomainEvent_When_AggregateStateChanges

**Scenario:**

- An aggregate must record a domain event whenever a significant domain action occurs.

**Arrange:**

- Create a test aggregate derived from `AggregateRoot`.
- Define a domain method that raises a test domain event.

**Act:**

- Execute the domain method.

**Assert:**

- The aggregate contains exactly one recorded domain event.
- The recorded event is of the expected type.

---

### TC-02: Should_ExposeRecordedDomainEvents_When_AggregateHasRaisedEvents

**Scenario:**

- Recorded domain events must be accessible so that upper layers can inspect or dispatch them later.

**Arrange:**

- Create an aggregate and trigger one or more domain events.

**Act:**

- Read the aggregate domain events collection.

**Assert:**

- The collection is not empty.
- It contains the expected events in the expected order.

---

### TC-03: Should_ClearDomainEvents_When_ClearDomainEventsIsCalled

**Scenario:**

- After domain events are dispatched or consumed, the aggregate must allow clearing the internal collection.

**Arrange:**

- Create an aggregate and record multiple domain events.

**Act:**

- Call the method responsible for clearing domain events.

**Assert:**

- The domain events collection becomes empty.

---

### TC-04: Should_ImplementDomainEventContract_When_DomainEventIsDefined

**Scenario:**

- Every domain event type must comply with the domain event contract.

**Arrange:**

- Define one or more test domain event implementations.

**Act:**

- Inspect their implemented interfaces and base types.

**Assert:**

- Each event implements the marker interface for domain events.
- If a base class is used, it provides the shared behavior consistently.

---

### TC-05: Should_NotCoupleAggregateToInfrastructure_When_RaisingDomainEvent

**Scenario:**

- Aggregates must only record domain events and must not dispatch them directly through infrastructure or application services.

**Arrange:**

- Inspect aggregate behavior and dependencies.

**Act:**

- Trigger domain logic that raises a domain event.

**Assert:**

- The event is only added to the aggregate collection.
- No infrastructure dispatch dependency is required by the aggregate.

---

### TC-06: Should_PreserveDomainEventData_When_EventIsRecordedByAggregate

**Scenario:**

- A domain event must retain the business data captured at the time of the domain action.

**Arrange:**

- Create an aggregate and raise a domain event containing meaningful payload data.

**Act:**

- Read the recorded event from the aggregate collection.

**Assert:**

- The event contains the expected domain data.
- The captured data matches the state transition that triggered the event.

---

### TC-07: Should_DispatchRecordedDomainEvents_When_PersistenceBoundaryProcessesAggregate

**Scenario:**

- Recorded domain events must remain available for later dispatch when the aggregate crosses the persistence boundary.

**Arrange:**

- Create and persist an aggregate that recorded domain events.
- Use the persistence mechanism responsible for domain event dispatch integration.

**Act:**

- Save changes through the persistence boundary.

**Assert:**

- Recorded domain events are discovered and processed by the dispatch mechanism.
- Aggregate event lifecycle remains consistent before and after processing.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that domain events do not expose sensitive infrastructure details or accidental technical metadata from lower layers.

Only business-relevant and intentionally modeled information should be captured.

### 3.2 Observability & Traceability

Verify that domain events can be traced from the aggregate action that raised them through later dispatch boundaries.

This is especially important for debugging workflows and understanding business state transitions.

### 3.3 Contract Stability

Verify that domain event contracts remain stable and predictable for internal consumers such as application handlers and persistence dispatchers.

Changes to event shape or semantics must be deliberate and backward-safe within the architecture.

---

## Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Test aggregate instance
  - `ProjectCreatedDomainEvent`
  - `OrderPlacedEvent`
  - Aggregate methods that trigger domain state changes

- **Expected Outputs:**
  - Domain event collection contains expected event instances
  - Event payload matches business action data
  - Event collection is empty after clearing
  - Persistence boundary can observe recorded events for dispatch

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Define failing tests for aggregate event recording, event exposure, and clearing behavior.
2. **GREEN:** Implement the minimal domain event contracts and aggregate collection logic.
3. **REFACTOR:** Improve naming, extraction, and collection handling while preserving green tests.

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
 │   └── Hector.BuildingBlocks.Domain.UnitTests/
 │       └── Primitives/
 │           ├── AggregateRootTests.cs
 │           └── DomainEventBaseTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── HectorDbContextDomainEventDispatchTests.cs
```

---

## Summary

This test plan ensures that Domain Events are modeled as stable, explicit, and decoupled business signals inside the domain layer. By validating event recording, exposure, lifecycle management, and persistence-boundary readiness, the architecture preserves one of the key DDD patterns for building extensible and loosely coupled domain models.

---
