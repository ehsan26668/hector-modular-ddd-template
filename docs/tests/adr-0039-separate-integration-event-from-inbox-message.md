# Test Plan: ADR-0039 Separate Integration Event from Inbox Message

## Status

Accepted

## Context

This test plan validates the **separation between Integration Events and Inbox Messages** described in [ADR-0039](/docs/adr/0039-separate-integration-event-from-inbox-message.md).  

This ADR is critical because it prevents a leak of abstraction between producer-side messaging responsibilities and consumer-side message processing. In the previous design, the publishing API accepted `IInboxMessage`, which introduced a dependency from the producer side into a concept that belongs strictly to consumers.

The architecture requires a clear separation of concerns:

- Producers publish **Integration Events**
- Consumers persist and process **Inbox Messages**

Without this separation, the Application layer risks depending on infrastructure concepts, violating modular boundaries and weakening the domain model purity.

This ADR introduces the `IIntegrationEvent` marker interface and updates `IIntegrationEventBus.PublishAsync` to depend only on this abstraction. The behavior must be validated to ensure the publishing pipeline depends exclusively on `IIntegrationEvent`, and Inbox abstractions remain strictly consumer-side concerns.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on isolated business logic, contract shape, mapping rules, and state transitions.
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Messaging.UnitTests`

- **Integration Tests:**
  - Focus on end-to-end request pipeline behavior, persistence consistency, middleware interactions, or cross-module communication.
  - Target Project: `tests/IntegrationTests/Hector.BuildingBlocks.Messaging.IntegrationTests`

---

## 1. Scope

- **Included:**
  - Validation that integration events implement the `IIntegrationEvent` marker interface.
  - Verification that `IIntegrationEventBus.PublishAsync` depends on `IIntegrationEvent`.
  - Enforcement that producer-side components do not depend on `IInboxMessage`.

- **Excluded:**
  - Inbox message persistence and consumer processing logic.
  - Message broker delivery guarantees.

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_ImplementIntegrationEventMarker_When_DefiningIntegrationEvent

**Scenario:** Integration events must implement the `IIntegrationEvent` marker interface to be eligible for publication.

**Arrange:**

- Create or reference a sample integration event such as `ProjectCreatedIntegrationEvent`.
- Inspect its implemented interfaces.

**Act:**

- Evaluate the type hierarchy of the event class.

**Assert:**

- Verify the event implements `IIntegrationEvent`.
- Verify that `IIntegrationEvent` extends `INotification`.

### TC-02: Should_AcceptIntegrationEvent_When_PublishAsyncIsCalled

**Scenario:** The Event Bus should accept events that implement `IIntegrationEvent`.

**Arrange:**

- Create a mock or test implementation of `IIntegrationEventBus`.
- Create a valid integration event instance implementing `IIntegrationEvent`.

**Act:**

- Call `PublishAsync` with the integration event instance.

**Assert:**

- Verify the event is accepted for publication.
- Verify no dependency on `IInboxMessage` exists in the publish API.

### TC-03: Should_NotDependOnInboxMessage_When_AnalyzingProducerAssemblies

**Scenario:** Producer-side assemblies must not reference the `IInboxMessage` abstraction.

**Arrange:**

- Configure architectural tests using `NetArchTest.Rules`.
- Define the producer scope (Application layer or event publishing components).

**Act:**

- Execute architecture rules checking dependencies against inbox-related abstractions.

**Assert:**

- Verify that no producer-side type references `IInboxMessage`.
- Verify inbox abstractions exist only in consumer-related components.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that no sensitive information, stack traces, connection strings, or internal IDs leak to external layers or clients.
- Verify that internal-only fields are not exposed through contracts or serialized payloads.

### 3.2 Observability & Traceability

- Verify that logging, metrics, and correlation/trace IDs are preserved and correctly propagated across boundaries.
- Verify that event publication and persistence can be traced end-to-end.

### 3.3 Contract Stability

- Verify that API or event contracts remain stable and predictable for consumers.
- Verify that contract names, versions, and shapes follow the approved architectural rules.

---

## 4. Test Data

- **Inputs:**
  - `ProjectCreatedIntegrationEvent` implementing `IIntegrationEvent`
  - A legacy object not implementing `IIntegrationEvent`
  - Producer assembly types referencing the event bus

- **Expected Outputs:**
  - Successful publication of valid integration events
  - Compile-time or runtime rejection of invalid event types
  - Architectural validation confirming absence of `IInboxMessage` dependencies

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED**
   - Define failing tests asserting that publishing requires `IIntegrationEvent`.
   - Detect usage of `IInboxMessage` within producer-side assemblies.

2. **GREEN**
   - Introduce `IIntegrationEvent`.
   - Update `IIntegrationEventBus.PublishAsync` to require `IIntegrationEvent`.

3. **REFACTOR**
   - Remove leftover `IInboxMessage` references from publishing components.
   - Improve naming and interface clarity across the messaging abstractions.

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
 │   └── Hector.BuildingBlocks.Messaging.UnitTests/
 │       └── IntegrationEvents/
 │           └── IntegrationEventContractTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Messaging.IntegrationTests/
         └── IntegrationEventBusPublishTests.cs
```

## Summary

This test plan ensures that ADR-0039 is validated against the expected architectural and runtime behavior.
The result should improve system quality, reliability, and maintainability while preserving the modular boundaries defined by the architecture.
