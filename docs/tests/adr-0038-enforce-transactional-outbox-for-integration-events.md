# Test Plan: ADR-0038 Enforce Transactional Outbox for Integration Events

## Status

Accepted

## Context

This test plan validates the **enforcement of Transactional Outbox exclusively for Integration Events and the prohibition of persisting Domain Events as outbox messages** described in [ADR-0038](/docs/adr/0038-enforce-transactional-outbox-for-integration-events.md).  

This ADR is critical because it preserves strict architectural boundaries between Domain, Application, Infrastructure, and Messaging layers in a Modular DDD architecture.  

It ensures:

- Domain events remain internal in-process notifications.
- Integration events are the only durable external contracts.
- Aggregate state changes and integration publication intent remain atomic.
- No internal domain model details leak into external messaging contracts.
- External publication never occurs directly from the request thread.

The behavior must be validated to guarantee architectural purity, transactional consistency, and reliable asynchronous publication via the outbox processor.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on domain event collection, dispatching behavior, integration event creation, and prevention of direct domain event persistence.
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Focus on EF Core save pipeline behavior, atomic persistence of aggregates and outbox messages, and ensuring no direct broker publication occurs.
  - Target Project: `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

- **Included:**
  - Enforcement that only integration events are persisted as outbox messages.
  - Domain event dispatch inside the save pipeline.
  - Atomic persistence of aggregate changes and integration events.
  - Prevention of direct broker publication from request thread.

- **Excluded:**
  - Actual message broker delivery guarantees.
  - Consumer-side inbox handling logic.

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_DispatchDomainEventsInProcess_When_SavingAggregate

**Scenario:** Domain events raised by aggregates must be dispatched in-process before committing changes.

**Arrange:**

- Create aggregate that raises a domain event.
- Mock `IDomainEventDispatcher`.
- Configure `HectorDbContext` with dispatcher.

**Act:**

- Call `SaveChangesAsync()`.

**Assert:**

- Verify dispatcher is invoked with the raised domain event.
- Verify dispatch occurs before transaction commit.
- Verify domain events are cleared only after successful save.

### TC-02: Should_PersistIntegrationEventAsOutboxMessage_When_HandlerPublishesIntegrationEvent

**Scenario:** A domain event handler creates an integration event via `IIntegrationEventBus`.

**Arrange:**

- Create aggregate that raises domain event.
- Configure handler that calls `IIntegrationEventBus.PublishAsync`.
- Use persistence implementation of `IIntegrationEventBus`.

**Act:**

- Call `SaveChangesAsync()`.

**Assert:**

- Verify integration event is stored as an outbox message.
- Verify aggregate state changes and outbox message are committed atomically.
- Verify no external broker call is executed.

### TC-03: Should_NotPersistDomainEventAsOutboxMessage_When_SavingAggregate

**Scenario:** Domain events must never be serialized or stored as outbox messages directly.

**Arrange:**

- Create aggregate with domain event.
- Do not register any integration event handler.
- Configure full persistence pipeline.

**Act:**

- Call `SaveChangesAsync()`.

**Assert:**

- Verify no outbox message exists for the domain event.
- Verify domain event type is not serialized.
- Verify no contract metadata is generated for domain events.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that internal domain event properties are never exposed through serialized outbox payloads.
- Verify that only integration event contracts from Contracts assemblies are serialized.
- Verify no stack traces or internal infrastructure details leak to outbox storage.

### 3.2 Observability & Traceability

- Verify that correlation IDs are preserved from request context to integration event metadata.
- Verify that outbox records contain traceable metadata for asynchronous publication.
- Verify logging distinguishes domain dispatch from integration persistence.

### 3.3 Contract Stability

- Verify that only integration events defined in Contracts assemblies are persisted.
- Verify that domain events do not require event names, versions, or transport metadata.
- Verify that event naming follows ADR-0030 rules.

---

## 4. Test Data

- **Inputs:**
  - Aggregate raising a sample domain event.
  - Domain event handler translating to `ProjectCreatedIntegrationEvent`.
  - Aggregate with no integration event bridge.

- **Expected Outputs:**
  - Domain event dispatched in-process only.
  - Integration event stored as outbox message.
  - No outbox record when only domain event exists.

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED**
   - Define failing tests asserting that domain events are not persisted as outbox messages.
   - Define failing tests asserting integration events are stored atomically.

2. **GREEN**
   - Implement save pipeline enforcing domain dispatch and integration persistence separation.
   - Ensure outbox persistence only accepts integration events.

3. **REFACTOR**
   - Improve separation between dispatcher and integration event bus.
   - Simplify pipeline orchestration while preserving transactional guarantees.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Security and non-functional points are verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       └── Outbox/
 │           └── TransactionalOutboxEnforcementTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── TransactionalOutboxPipelineTests.cs

## Summary

This test plan ensures that ADR-0038 is validated against the expected architectural and runtime behavior.
The result preserves domain model purity, enforces transactional consistency, and guarantees that integration events are the only durable external messaging contracts within the system.
