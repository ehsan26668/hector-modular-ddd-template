# Test Plan: ADR-0028 Integration Event Bus Abstraction

## Status

Accepted (Superseded by [ADR-0027](/docs/adr/0027-domain-event-to-integration-event-bridge.md) and [ADR-0039](/docs/adr/0039-separate-integration-event-from-inbox-message.md))

## Context

This test plan validates the **Integration Event Bus Abstraction** (`IIntegrationEventBus`) described in [ADR-0028](/docs/adr/0028-integration-event-bus-abstraction.md). Although ADR-0028 has been superseded and refined by [ADR-0027](/docs/adr/0027-domain-event-to-integration-event-bridge.md) and [ADR-0039](/docs/adr/0039-separate-integration-event-from-inbox-message.md), validating this abstraction remains critical. It guarantees that the Application layer remains completely decoupled from concrete messaging infrastructure (such as RabbitMQ, Kafka, or InMemory brokers) and that the default mediator-backed implementation behaves predictably without leaking transport details.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on verifying that the default `InMemoryIntegrationEventBus` (or `OutboxIntegrationEventBus`) correctly routes integration events to the underlying mediator/outbox store and propagates cancellation tokens.
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Focus on ensuring that the registered implementation of `IIntegrationEventBus` in the Dependency Injection container resolves correctly and successfully pushes events through the pipeline within a real application context.
  - Target Project: `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

- **Included:**
  - The behavior of the `IIntegrationEventBus` interface and its concrete modular monolith implementations.
  - Verification of call forwarding to `IMediator` or `Outbox` depending on the active implementation.
  - Propagation of `CancellationToken` throughout the publishing pipeline.
  - Architecture Guard tests verifying that no Domain assembly references `IIntegrationEventBus`.

- **Excluded:**
  - Real external message broker integration (e.g., connection recovery in RabbitMQ).
  - Outbox processing schedules and lock management (covered in ADR-0022 and ADR-0024).

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_ForwardEventToMediator_When_PublishingViaInMemoryBus

**Scenario:** Validate that the in-memory event bus implementation correctly routes integration events to the mediator.

**Arrange:**

- Mock `IMediator` to capture published notifications.
- Instantiate `InMemoryIntegrationEventBus` using the mocked mediator.
- Create a test integration event implementing `IIntegrationEvent` (which inherits from `INotification`).

**Act:**

- Call `IIntegrationEventBus.PublishAsync(integrationEvent, cancellationToken)`.

**Assert:**

- Verify `IMediator.PublishAsync` was called exactly once with the same integration event instance and correlation token.

---

### TC-02: Should_ThrowException_When_PublishingNullEvent

**Scenario:** Ensure the bus safeguards the pipeline by preventing null events from being published.

**Arrange:**

- Instantiate `InMemoryIntegrationEventBus` with a mocked mediator.

**Act:**

- Invoke `PublishAsync` passing `null` as the integration event.

**Assert:**

- Verify that an `ArgumentNullException` is thrown.

---

### TC-03: Should_ResolveConcreteBusImplementation_FromDependencyInjection

**Scenario:** Verify that the DI container correctly registers and resolves `IIntegrationEventBus` to the expected concrete implementation.

**Arrange:**

- Setup a Test Host / Service Collection containing the persistence and application building blocks.

**Act:**

- Resolve `IIntegrationEventBus` from the `IServiceProvider`.

**Assert:**

- The resolved instance must not be null.
- The resolved instance must match the configured implementation (e.g., `OutboxIntegrationEventBus` or `InMemoryIntegrationEventBus`).

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that no infrastructure connection details (e.g., broker connection strings) are exposed in error messages or logs thrown by the event bus abstraction.

### 3.2 Observability & Traceability

- Verify that the calling context's `CorrelationId` is successfully preserved when `IIntegrationEventBus.PublishAsync` is invoked.
- Ensure that the bus emits structured information logs before and after dispatching the event.

### 3.3 Contract Stability

- Verify through architecture tests that `IIntegrationEventBus` and `IIntegrationEvent` remain in the `Hector.BuildingBlocks.Application` project, and that domain layer projects do not reference them.

---

## 4. Test Data

- **Inputs:**
  - A mock integration event: `TestIntegrationEvent(Guid Id, string Data)`
  - An empty/cancelled `CancellationToken` to verify circuit breaking.

- **Expected Outputs:**
  - Successful dispatch invocation on `IMediator` or record creation in the outbox.
  - Clean error tracking when cancellation is requested.

---

## 5. TDD Execution Plan

1. **RED:**
   - Write a unit test that verifies `IIntegrationEventBus` forwards notifications to the mediator, which fails because the abstraction or concrete registration is missing.

2. **GREEN:**
   - Define the `IIntegrationEventBus` interface and implement the default in-memory handler.

3. **REFACTOR:**
   - Refactor implementation to leverage assembly scanning and clean DI extensions, ensuring no direct reference to transport details inside the application layer.

---

## 6. Exit Criteria

- [x] All Unit Tests for `InMemoryIntegrationEventBus` and `OutboxIntegrationEventBus` pass.
- [x] DI registration integration tests pass.
- [x] Architecture tests confirm that the Domain layer has zero references to `IIntegrationEventBus`.
- [x] Decision log and documentation references updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       └── Outbox/
 │           └── OutboxIntegrationEventBusTests.cs
 ├── IntegrationTests/
 │   └── Hector.BuildingBlocks.Persistence.IntegrationTests/
 │       └── IntegrationEventBusRegistrationTests.cs
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         └── IntegrationEventBusContractTests.cs
```

## Summary

This test plan ensures that the IIntegrationEventBus abstraction defined in [ADR-0028](/docs/adr/0028-integration-event-bus-abstraction.md) decouples the core application logic from messaging infrastructure. By verifying the isolation of the abstraction, we keep the architecture flexible, enabling future transition to external brokers like RabbitMQ or Kafka without modifying domain or application logic.
