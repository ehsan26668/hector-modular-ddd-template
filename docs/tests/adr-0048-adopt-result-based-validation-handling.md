# Test Plan: ADR-0048 Adopt Result-Based Validation Handling

## Status

Accepted

## Context

This test plan validates the **Result-based validation handling strategy** introduced in [ADR-0048](/docs/adr/0048-adopt-result-based-validation-handling.md).

Application commands and queries are validated using pipeline behaviors.

Traditionally, validation frameworks such as FluentValidation throw exceptions when validation fails:

FluentValidation.ValidationException

Using exceptions for expected validation failures introduces several architectural issues:

- validation failures become indistinguishable from unexpected failures
- exception-based control flow increases complexity
- exception handling leaks into higher layers
- handlers may execute even when validation fails

ADR‑0047 introduced a standardized Result pattern for the Application Layer.

ADR‑0048 integrates validation into this Result-based error flow.

Validation failures must therefore return:

`Result.Failure(ValidationError)`

or

`Result<T>.Failure(ValidationError)`

instead of throwing exceptions.

The validation pipeline must also ensure that request handlers do not execute when validation fails.

Additionally, the ADR defines the required pipeline order:

1. Correlation behavior
2. Inbox behavior
3. Validation behavior
4. Transaction behavior
5. Handler execution

This test plan verifies correct validation behavior, Result integration, and pipeline execution order.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests**
  - Validate validation error creation and Result failure generation.
  - Target Project: `tests/UnitTests/Hector.Application.UnitTests`

- **Integration Tests**
  - Validate validation pipeline behavior and handler execution blocking.
  - Target Project: `tests/IntegrationTests/Hector.Application.IntegrationTests`

- **Architecture Tests**
  - Ensure validators do not throw exceptions.
  - Validate pipeline behavior ordering.
  - Target Project: `tests/ArchitectureTests`

---

## 1. Scope

- **Included:**
  - Validation pipeline execution
  - Result-based validation failures
  - Validation error aggregation
  - Handler execution prevention on validation failure
  - Validation error structure
  - Pipeline execution order

- **Excluded:**
  - Domain validation logic
  - Error taxonomy classification (ADR‑0050)
  - API response formatting

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_ReturnFailureResult_When_ValidationFails

**Scenario:**  
A request fails validation due to invalid input.

**Arrange:**

Create a command with invalid properties and a registered validator.

**Act:**

Execute the request through the application pipeline.

**Assert:**

- Verify the response is `Result.Failure`.
- Verify the failure contains a `ValidationError`.
- Verify validation errors are included in the response.

---

### TC-02: Should_ReturnFailureResultWithValidationErrors_When_MultipleErrorsExist

**Scenario:**  
Multiple validation failures occur for a single request.

**Arrange:**

Create a request violating multiple validation rules.

**Act:**

Execute the request through the validation pipeline.

**Assert:**

- Verify the response is `Result.Failure`.
- Verify all validation errors are aggregated.
- Verify each error includes property name and message.

---

### TC-03: Should_NotExecuteHandler_When_ValidationFails

**Scenario:**  
When validation fails, the request handler must not execute.

**Arrange:**

Create a handler that modifies observable state.
Create an invalid request.

**Act:**

Execute the request.

**Assert:**

- Verify validation failure is returned.
- Verify the handler method was not executed.
- Verify observable state remains unchanged.

---

### TC-04: Should_ExecuteHandler_When_RequestIsValid

**Scenario:**  
A valid request passes validation and reaches the handler.

**Arrange:**

Create a valid command and validator.

**Act:**

Execute the request through the pipeline.

**Assert:**

- Verify the handler executes successfully.
- Verify the result is `Result.Success`.

---

### TC-05: Should_EnsureValidationErrorsContainStructuredInformation

**Scenario:**  
Validation errors must contain structured metadata.

**Arrange:**

Create a validator that produces validation failures.

**Act:**

Execute the request.

**Assert:**

Verify each validation error includes:

- error code
- error message
- property name
- validation details

---

### TC-06: Should_EnsureValidationBehaviorExecutesBeforeHandler

**Scenario:**  
Validation must occur before handler execution.

**Arrange:**

Register validation behavior and handler.

**Act:**

Execute an invalid request.

**Assert:**

- Verify validation executes before handler invocation.
- Verify handler is skipped when validation fails.

---

### TC-07: Should_EnsureValidationOccursBeforeTransactionBehavior

**Scenario:**  
Validation must execute before transactional operations begin.

**Arrange:**

Register transaction and validation pipeline behaviors.

**Act:**

Execute an invalid command.

**Assert:**

- Verify validation failure occurs before transaction starts.
- Verify no transaction is opened for invalid requests.

---

### TC-08: Should_ConvertUnexpectedValidatorExceptionsIntoFailureResult

**Scenario:**  
A validator throws an unexpected exception.

**Arrange:**

Create a validator that throws an exception.

**Act:**

Execute the request through the application pipeline.

**Assert:**

- Verify the exception does not propagate to the caller.
- Verify the response is `Result.Failure`.
- Verify the error type represents an unexpected failure.

---

## 3. Non-Functional Validation Points

### 3.1 Consistency

- Verify validation failures follow the Result contract.
- Verify handlers do not receive invalid requests.

### 3.2 Observability

- Verify validation failures contain structured error information.
- Verify errors can be logged and traced.

### 3.3 Architectural Integrity

- Verify validation integrates with the Result pattern.
- Verify exception-based control flow is eliminated for validation failures.

---

## 4. Test Data

- **Inputs:**
  - Invalid commands
  - Commands with multiple validation failures
  - Valid commands
  - Validator throwing exception

- **Expected Outputs:**
  - Result.Failure(ValidationError)
  - Aggregated validation error list
  - Handler skipped when validation fails
  - Result.Success for valid requests

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. RED

Write failing tests for:

- validation failure returning Result
- handler execution prevention
- validation error aggregation
- pipeline ordering

1. GREEN

Implement:

- validation pipeline behavior
- validation error structure
- Result-based failure responses

1. REFACTOR

Simplify validator integration and ensure consistent validation handling across commands and queries.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Validation failures return Result failures.
- [ ] Handlers do not execute when validation fails.
- [ ] Pipeline ordering validated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Application.UnitTests/
 │       ├── ValidationErrorTests.cs
 │       └── ValidationResultTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.Application.IntegrationTests/
 │       └── ValidationPipelineTests.cs
 │
 └── ArchitectureTests/
     ├── ValidationBehaviorOrderTests.cs
     └── ValidatorExceptionHandlingTests.cs
```

## Summary

This test plan validates the Result-based validation handling strategy defined in ADR‑0048.

It ensures that:

- validation failures are returned using Result failures
- handlers do not execute when validation fails
- validation errors are structured and aggregated
- validation integrates cleanly with the Result pattern

This approach eliminates exception-based control flow for validation failures and ensures consistent error handling across the Application Layer.
