# Test Plan: ADR-0014 Adopt Internal Mediator for CQRS

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0014](/docs/adr/0014-adopt-internal-mediator-for-CQRS.md).

[ADR-0014](/docs/adr/0014-adopt-internal-mediator-for-CQRS.md) introduces a lightweight internal mediator implementation inside `Hector.BuildingBlocks.Application` instead of relying on external libraries such as MediatR.

The mediator is responsible for:

- dispatching commands to their corresponding command handlers,
- dispatching queries to their corresponding query handlers,
- supporting pipeline behaviors for cross‑cutting concerns,
- integrating with the dependency injection container.

This decision is critical for:

- maintaining architectural control,
- reducing external dependencies,
- enabling predictable CQRS execution,
- supporting pipeline-based cross-cutting concerns (validation, transactions, logging).

This test plan ensures that the internal mediator:

- correctly dispatches requests,
- enforces handler contracts,
- executes pipeline behaviors in the correct order,
- integrates properly with DI,
- and does not depend on external mediator libraries.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on mediator dispatch behavior, handler resolution, pipeline execution order, cancellation propagation, and contract enforcement.

  - **Target Project:** `tests/UnitTests/Hector.BuildingBlocks.Application.UnitTests`

- **Integration Tests:**
  - Focus on end‑to‑end command/query execution within a module, DI integration, and interaction with transaction pipeline behaviors.

  - **Target Project:** `tests/IntegrationTests/Hector.Modules.Projects.IntegrationTests`

---

## 1. Scope

### Included

- Dispatching `ICommand` to `ICommandHandler<TCommand>`
- Dispatching `IQuery<TResult>` to `IQueryHandler<TQuery, TResult>`
- Resolution of handlers via dependency injection
- Execution of `IPipelineBehavior<TRequest, TResponse>`
- Correct pipeline execution order (outer → inner → handler → unwind)
- Cancellation token propagation
- Ensuring exactly one handler per request type
- Verification that no external mediator library is required
- Integration of mediator within application modules

### Excluded

- HTTP endpoint wiring
- Outbox processing and background services
- Distributed messaging concerns
- Validation logic correctness (covered in specific behavior tests)
- Performance benchmarking

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_DispatchCommandToMatchingHandler_When_CommandIsSent

**Scenario:**

- A command implementing `ICommand` should be dispatched to its corresponding `ICommandHandler<TCommand>`.

**Arrange:**

- Define a test command implementing `ICommand`.
- Register a corresponding command handler in DI.
- Resolve IMediator.

**Act:**

- Call `mediator.Send(command)`.

**Assert:**

- The correct handler is invoked.
- The handler logic executes successfully.

---

### TC-02

- #### Should_DispatchQueryToMatchingHandler_When_QueryIsSent

**Scenario:**

- A query implementing `IQuery<TResult>` should be dispatched to its corresponding handler and return the expected result.

**Arrange:**

- Define a test query implementing `IQuery<TResult>`.
- Register a corresponding query handler.
- Resolve `IMediator`.

**Act:**

- Call `mediator.Send(query)`.

**Assert:**

- The correct handler is invoked.
- The returned result matches the expected value.

---

### TC-03

- #### Should_ExecutePipelineBehaviorsInCorrectOrder_When_RequestIsHandled

**Scenario:**

- Pipeline behaviors must wrap request execution in the correct order.

**Arrange:**

- Register multiple test pipeline behaviors.
- Register a test handler.
- Track execution order.

**Act:**

- Send a request through the mediator.

**Assert:**

- Behaviors execute in outer-to-inner order before the handler.
- Control returns in reverse order after handler execution.
- The execution chain is deterministic and predictable.

---

### TC-04

- #### Should_InvokeHandlerOnlyOnce_When_RequestIsSent

**Scenario:**

- Each request type must be handled by exactly one handler.

**Arrange:**

- Register a single handler for a test command.

**Act:**

- Send the command via mediator.

**Assert:**

- The handler is invoked exactly once.
- No duplicate invocation occurs.

---

### TC-05

- #### Should_ThrowException_When_NoHandlerIsRegisteredForRequest

**Scenario:**

- If no handler is registered for a request, mediator should fail deterministically.

**Arrange:**

- Register mediator without a handler for a specific request type.

**Act:**

- Send the request.

**Assert:**

- An appropriate exception is thrown.
- The failure clearly indicates missing handler configuration.

---

### TC-06

- #### Should_PropagateCancellationToken_When_RequestIsCancelled

**Scenario:**

- Cancellation tokens must propagate through mediator, behaviors, and handlers.

**Arrange:**

- Define a handler that captures the cancellation token.
- Create a cancelled `CancellationToken`.

**Act:**

- Call `mediator.Send(request, cancellationToken)`.

**Assert:**

- The cancellation token received by the handler matches the original.
- Operation respects cancellation.

---

### TC-07

- #### Should_IntegrateWithDependencyInjection_When_MediatorIsResolved

**Scenario:**

- Mediator and handlers must be resolved through DI container.

**Arrange:**

- Configure services using mediator registration extensions.
- Register handlers and pipeline behaviors.

**Act:**

- Resolve `IMediator` from service provider.
- Send a request.

**Assert:**

- Mediator is resolved successfully.
- Handler resolution occurs via DI.
- All dependencies are injected correctly.

---

### TC-08

- #### Should_NotDependOnExternalMediatorLibrary_When_InternalMediatorIsUsed

**Scenario:**

- The architecture must not depend on MediatR or other external mediator libraries.

**Arrange:**

- Inspect application layer dependencies.

**Act:**

- Evaluate referenced assemblies and namespaces.

**Assert:**

- No reference to MediatR exists.
- Only internal mediator abstractions are used.

---

### TC-09

- #### Should_WorkWithTransactionPipelineBehavior_When_CommandIsExecuted

**Scenario:**

- Mediator must support pipeline behaviors such as transaction handling.

**Arrange:**

- Register transaction pipeline behavior.
- Register a command handler that modifies persistence state.

**Act:**

- Send the command through mediator.

**Assert:**

- Transaction behavior wraps handler execution.
- Changes are committed within transaction scope.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that mediator exceptions do not expose sensitive infrastructure details or internal service registrations beyond intended layers.

### 3.2 Observability & Traceability

Verify that mediator execution allows correlation and tracing behaviors to propagate context across pipeline behaviors and handlers.

### 3.3 Contract Stability

Verify that mediator interfaces (`ICommand`, `IQuery`, `IMediator`, `IPipelineBehavior`) remain stable and predictable across modules.

Changes to mediator contracts must not break existing application handlers.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Test command implementing `ICommand`
  - Test query implementing `IQuery<TResult>`
  - Multiple pipeline behaviors
  - Cancellation tokens
  - Registered and unregistered handlers

- **Expected Outputs:**
  - Correct handler invocation
  - Correct result returned for queries
  - Deterministic pipeline execution order
  - Proper exception for missing handlers
  - No dependency on external mediator library

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Write failing tests for command/query dispatching and pipeline execution.
2. **GREEN:** Implement minimal internal mediator capable of resolving handlers and executing pipeline behaviors.
3. **REFACTOR:** Improve handler resolution logic, optimize pipeline chaining, and ensure clean separation between mediator and DI infrastructure.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Security/Non-functional points verified.
- [ ] No dependency on external mediator libraries.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Application.UnitTests/
 │       ├── Messaging/
 │       │   ├── MediatorTests.cs
 │       │   ├── PipelineBehaviorTests.cs
 │       │   └── HandlerResolutionTests.cs
 └── IntegrationTests/
     └── Hector.Modules.Projects.IntegrationTests/
         └── MediatorIntegrationTests.cs
```

---

## Summary

This test plan validates the adoption of an internal mediator as the central CQRS coordination mechanism within the Hector framework. It ensures deterministic request dispatching, predictable pipeline behavior execution, proper DI integration, and architectural independence from external mediator libraries. The result is a lightweight, controlled, and extensible request execution pipeline aligned with the BuildingBlocks philosophy.

---
