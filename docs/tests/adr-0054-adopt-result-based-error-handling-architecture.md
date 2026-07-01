# Test Plan: ADR-0054 Adopt Result-Based Error Handling Architecture

## Status

Accepted

## Context

This test plan validates the end-to-end integration and architectural integrity of the Result-based error handling architecture introduced in [ADR-0054](/docs/adr/0054-adopt-result-based-error-handling-architecture.md).

ADR-0054 aggregates and standardizes the decisions from:

- ADR-0047 (Result Pattern)
- ADR-0048 (Validation)
- ADR-0049 (Queries)
- ADR-0050 (Taxonomy)
- ADR-0051 (CQRS Allowed Categories)
- ADR-0052 (Error Object Model)
- ADR-0053 (Error Factory & Codes)

To ensure this architecture operates correctly as a unified system, we must validate the entire pipeline flow—from request entry, through validation and handler execution, to final result translation.

## Test Strategy

Define the layers of testing to be used:

- **Integration Tests**
  - Validate the behavior of the application pipeline (Pipeline Behaviors).
  - Verify that validation failures, domain exceptions, and database conflicts are successfully intercepted and mapped to the correct Result.Failure outputs.
  - Target Project: `tests/IntegrationTests/Hector.Application.IntegrationTests`

- **Architecture Tests**
  - Enforce that all Command and Query handlers return Result or `Result<T>`.
  - Enforce that the Domain Layer has zero dependencies on Result/Error structures.
  - Enforce that exceptions do not leak from the Application Layer.
  - Target Project: `tests/ArchitectureTests`

- **System / API Mapping Tests (Contract Tests)**
  - Validate that Results map correctly to proper HTTP Status Codes in the presentation layer.
  - Target Project: `tests/IntegrationTests/Hector.Api.IntegrationTests`

---

## 1. Scope

- **Included:**
  - End-to-end application pipeline execution.
  - Pipeline behavior ordering (Validation -> Logging -> Transaction).
  - Proper mapping of exceptions (Domain, Validation, Concurrency) to categorized Errors.
  - Strict return-type enforcement for all handlers.
  - Zero-exception leakage policy.

- **Excluded:**
  - Individual domain model invariant rules.
  - Detailed database transaction rollbacks (covered in transactional tests).

---

## 2. Test Cases (Integration / Architecture / System)

### TC-01: Should_ReturnValidationResultFailure_WhenRequestIsInvalid

**Scenario:**  
Invalid requests must be intercepted by the validation pipeline behavior and return a Validation failure without executing the handler.

**Arrange:**

- Register a command validator requiring a non-empty field.
- Create an invalid command (field empty).

**Act:**

- Send command through MediatR pipeline.

**Assert:**

- Verify execution returns `Result.Failure`.
- Verify error category is `ErrorCategory.Validation`.
- Verify handler was NOT executed (mock verify).

---

### TC-02: Should_MapDomainExceptionToBusinessRuleResult_WhenHandlerThrowsDomainException

**Scenario:**  
Domain exceptions thrown inside handlers must be caught by pipeline behaviors and returned as BusinessRule failures.

**Arrange:**

- Mock handler to throw a domain exception (e.g., `InvalidStatusException`).

**Act:**

- Send command through pipeline.

**Assert:**

- Verify returned result is `Result.Failure`.
- Verify error category is `ErrorCategory.BusinessRule`.

---

### TC-03: Should_ReturnConflictResult_WhenConcurrencyExceptionOccurs

**Scenario:**  
Database concurrency conflicts must be mapped to Conflict failures.

**Arrange:**

- Mock repository or DbContext to throw `DbUpdateConcurrencyException`.

**Act:**

- Send command through pipeline.

**Assert:**

- Verify returned result is `Result.Failure`.
- Verify error category is `ErrorCategory.Conflict`.

---

### TC-04: Should_ReturnUnexpectedResult_WhenUnhandledRuntimeExceptionOccurs

**Scenario:**  
Any unexpected runtime exception must be safely caught and mapped to an Unexpected error.

**Arrange:**

- Mock handler or service to throw `InvalidOperationException`.

**Act:**

- Send query/command through pipeline.

**Assert:**

- Verify no exception escapes the pipeline.
- Verify returned result is `Result.Failure`.
- Verify error category is `ErrorCategory.Unexpected`.

---

### TC-05: Should_EnforceAllHandlersToReturnResultTypes

**Scenario:**  
Architecture validation to ensure all MediatR Handlers implement `IRequestHandler<TRequest, Result>` or `IRequestHandler<TRequest, Result<TResponse>>`.

**Arrange:**

- Load Application assembly.

**Act:**

- Run NetArchTest rules on all classes implementing `IRequestHandler`.

**Assert:**

- Assert that handler return types must be `Result` or `Result<>`.

---

### TC-06: Should_PreventDomainLayerFromUsingResultOrError

**Scenario:**  
The Domain Layer must not contain references to the Application Layer's Result pattern or Error models.

**Arrange:**

- Load Domain assembly.

**Act:**

- Analyze type references.

**Assert:**

- Assert that Domain has no dependency on namespaces containing `Result` or `Errors`.

---

### TC-07: Should_MapResultToCorrectHttpResponses

**Scenario:**  
API controllers/endpoints must map Result statuses to equivalent HTTP response codes.

**Arrange:**

- Setup `WebApplicationFactory` for Api project.
- Mock handlers to return various failure categories (NotFound, Forbidden, Unauthorized, Conflict).

**Act:**

- Send HTTP requests to endpoints.

**Assert:**

- Verify mapping:
  - Validation -> 400 Bad Request
  - NotFound -> 404 Not Found
  - Unauthorized -> 401 Unauthorized
  - Forbidden -> 403 Forbidden
  - Conflict -> 409 Conflict
  - BusinessRule -> 422 Unprocessable Entity
  - Unexpected -> 500 Internal Server Error

---

## 3. Non-Functional Validation Points

### 3.1 Resilience & Stability

- The system must never throw unhandled 500 HTML error pages to the client; all errors must match the API error contract.

### 3.2 Observability & Telemetry

- Pipeline behaviors must log mapped error codes and categories for telemetry tracking before returning the result failure.

---

## 4. Test Data

- **Inputs:**
  - Invalid Command payloads
  - Query parameters pointing to non-existent data
  - Exception triggers (Domain, Concurrency, Unhandled)
  
- **Expected Outputs:**
  - Mapped Result objects matching categories in ADR-0050.
  - Proper HTTP status codes in HTTP responses.

---

## 5. TDD Execution Plan

1. **RED:**
   - Write architecture tests to verify handler return types and Domain isolation.
   - Write pipeline integration tests expecting mapped Results from thrown exceptions.

2. **GREEN:**
   - Implement the Global exception handling pipeline behavior.
   - Ensure all handlers return `Result`/`Result<T>`.

3. **REFACTOR:**
   - Centralize exception-to-error mapping logic within the pipeline helper classes.

---

## 6. Exit Criteria

- [ ] All Handlers comply with the standard Result return type constraint.
- [ ] No exceptions leak out of the MediatR pipeline.
- [ ] Concurrency and domain exceptions are mapped correctly to their error categories.
- [ ] Domain assembly remains independent of Result structures.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── ArchitectureTests/
 │   ├── HandlerReturnTypeTests.cs
 │   └── DomainIsolationTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.Application.IntegrationTests/
 │       └── PipelineExceptionMappingTests.cs
 │
 └── ApiTests/
     └── Hector.Api.IntegrationTests/
         └── ErrorResponseMappingTests.cs
```

## Summary

This test plan validates the global Result-Based Error Handling Architecture of Hector (ADR-0054). It guarantees that all operational behaviors, domain logic boundaries, validation, and API presentation layers adhere to a unified error handling contract.
