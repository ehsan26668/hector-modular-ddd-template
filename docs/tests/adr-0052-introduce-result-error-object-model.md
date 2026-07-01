# Test Plan: ADR-0052 Introduce Result Error Object Model

## Status

Accepted

## Context

This test plan validates the standardized Error object model introduced in [ADR-0052](/docs/adr/0052-introduce-result-error-object-model.md).

Previous ADRs defined the behavior of error handling in the Application Layer:

- ADR‑0047 — Standardize Result Pattern
- ADR‑0048 — Result-based validation handling
- ADR‑0049 — Result-based query responses
- ADR‑0050 — Application error taxonomy
- ADR‑0051 — Allowed error categories for commands and queries

However, those ADRs did not define the concrete object model used to represent errors inside Result objects.

Without a canonical Error model:

- modules may define incompatible error structures
- error metadata becomes inconsistent
- API mapping becomes unreliable
- logging and observability lose structure
- architecture rules cannot enforce error usage

ADR‑0052 introduces a canonical Error object model and standard Result API.

All application failures must use this structure.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests**
  - Validate Error object behavior and Result API contracts.
  - Validate metadata handling.
  - Target Project: `tests/UnitTests/Hector.Application.UnitTests`

- **Integration Tests**
  - Validate exception translation through the application pipeline.
  - Validate Result propagation across application flows.
  - Target Project: `tests/IntegrationTests/Hector.Application.IntegrationTests`

- **Architecture Tests**
  - Enforce Error model usage across the Application Layer.
  - Enforce layer boundaries between Domain and Application.
  - Target Project: `tests/ArchitectureTests`

---

## 1. Scope

- **Included:**
  - Error object structure
  - ErrorCategory enum enforcement
  - Result and `Result<T>` API validation
  - Validation error metadata
  - Exception-to-error translation
  - Layer dependency rules

- **Excluded:**
  - HTTP response mapping
  - API presentation formatting
  - Domain invariant logic

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_CreateErrorWithRequiredFields

**Scenario:**  
An Error object must include required fields.

**Arrange:**

Create an Error instance.

**Act:**

Inspect its properties.

**Assert:**

Verify the Error contains:

- Code
- Message
- Category

Verify Metadata is optional.

---

### TC-02: Should_AllowOptionalMetadataInError

**Scenario:**  
Error objects may contain structured metadata.

**Arrange:**

Create Error with metadata dictionary.

**Act:**

Access metadata values.

**Assert:**

Verify metadata entries are preserved.

Verify metadata is read-only.

---

### TC-03: Should_ExposeCorrectPropertiesInResult

**Scenario:**  
Result objects must expose the standard API.

**Arrange:**

Create success and failure results.

**Act:**

Inspect Result properties.

**Assert:**

Verify Result exposes:

- IsSuccess
- IsFailure
- Error

Verify success results have null Error.

---

### TC-04: Should_CreateFailureResultWithError

**Scenario:**  
Failure results must include an Error instance.

**Arrange:**

Create Error instance.

**Act:**

Call Result.Failure(error).

**Assert:**

Verify:

- IsFailure is true
- Error equals provided error
- IsSuccess is false

---

### TC-05: Should_CreateSuccessResult

**Scenario:**  
Success results must not contain errors.

**Arrange:**

Call Result.Success().

**Act:**

Inspect result.

**Assert:**

Verify:

- IsSuccess is true
- IsFailure is false
- Error is null

---

### TC-06: Should_CreateSuccessResultWithValue

**Scenario:**  
`Result<T>` must return a value on success.

**Arrange:**

Create `Result<int>.Success(42)`.

**Act:**

Inspect result.

**Assert:**

Verify:

- IsSuccess is true
- Value equals 42
- Error is null

---

### TC-07: Should_CreateFailureResultWithGenericResult

**Scenario:**  
`Result<T>` failures must contain errors.

**Arrange:**

Create Error instance.

**Act:**

Call `Result<T>.Failure(error)`.

**Assert:**

Verify:

- IsFailure is true
- Error equals provided error

---

### TC-08: Should_DefineCorrectErrorCategoryEnum

**Scenario:**  
ErrorCategory enum must match taxonomy defined in ADR‑0050.

**Arrange:**

Inspect ErrorCategory enumeration.

**Act:**

Load enum values.

**Assert:**

Verify values include:

- Validation
- NotFound
- Unauthorized
- Forbidden
- Conflict
- BusinessRule
- Infrastructure
- Unexpected

Verify no additional values exist.

---

### TC-09: Should_AggregateValidationErrorsWithMetadata

**Scenario:**  
Validation failures may include multiple structured errors.

**Arrange:**

Simulate validation failures with metadata.

**Act:**

Create validation error result.

**Assert:**

Verify metadata includes fields such as:

- field
- attemptedValue
- validationRule

Verify aggregated errors are preserved.

---

### TC-10: Should_MapDomainException_ToBusinessRuleError

**Scenario:**  
Domain exceptions must translate to BusinessRule errors.

**Arrange:**

Throw DomainException from domain logic.

**Act:**

Execute application request.

**Assert:**

Verify Result.Failure returned.

Verify Error.Category equals BusinessRule.

---

### TC-11: Should_MapValidationException_ToValidationError

**Scenario:**  
Validation exceptions must map to Validation category.

**Arrange:**

Trigger FluentValidationException.

**Act:**

Execute request pipeline.

**Assert:**

Verify Result.Failure returned.

Verify Error.Category equals Validation.

---

### TC-12: Should_MapConcurrencyException_ToConflictError

**Scenario:**  
Concurrency exceptions must map to Conflict errors.

**Arrange:**

Simulate DbUpdateConcurrencyException.

**Act:**

Execute command.

**Assert:**

Verify Result.Failure returned.

Verify category equals Conflict.

---

### TC-13: Should_MapExternalFailures_ToInfrastructureError

**Scenario:**  
External dependency failures must map to Infrastructure.

**Arrange:**

Simulate database or external service failure.

**Act:**

Execute application request.

**Assert:**

Verify Result.Failure returned.

Verify category equals Infrastructure.

---

### TC-14: Should_MapUnhandledExceptions_ToUnexpectedError

**Scenario:**  
Unhandled exceptions must map to Unexpected errors.

**Arrange:**

Throw unexpected runtime exception.

**Act:**

Execute application request.

**Assert:**

Verify Result.Failure returned.

Verify category equals Unexpected.

---

### TC-15: Should_PreventDomainLayerFromReferencingResultOrError

**Scenario:**  
Domain Layer must remain independent from Result and Error objects.

**Arrange:**

Analyze Domain assembly dependencies.

**Act:**

Run architecture validation.

**Assert:**

Verify Domain does not reference:

- `Result`
- `Result<T>`
- `Error`

---

### TC-16: Should_EnsureApplicationLayerProducesResults

**Scenario:**  
Application handlers must return Result types.

**Arrange:**

Analyze Application handlers.

**Act:**

Run architecture tests.

**Assert:**

Verify handlers return:

`Result`  
or  
`Result<T>`

---

## 3. Non-Functional Validation Points

### 3.1 Consistency

- Verify all application failures use the standardized Error model.
- Verify error metadata structure remains predictable.

### 3.2 Observability

- Verify errors expose structured information suitable for logging and monitoring.

### 3.3 Architectural Integrity

- Verify strict separation between Domain and Application layers.
- Verify consistent Result usage across handlers.

### 3.4 Contract Stability

- Verify Error structure remains stable across releases.

---

## 4. Test Data

- **Inputs:**
  - Valid Error objects
  - Errors with metadata
  - DomainException
  - FluentValidationException
  - DbUpdateConcurrencyException
  - External dependency failures
  - Unexpected runtime exceptions

- **Expected Outputs:**
  - `Result.Success`
  - `Result<T>.Success`
  - `Result.Failure(Error)`
  - Structured Error objects
  - Correct ErrorCategory values

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. RED

Write failing tests for:

- Error object structure
- Result API contracts
- ErrorCategory enum validation
- exception translation

1. GREEN

Implement:

- Error object
- Result and `Result<T>`
- exception mapping logic

1. REFACTOR

Improve Result ergonomics and reduce duplication in error creation.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Architecture tests enforce layer boundaries.
- [ ] Error object model validated.
- [ ] Result API contract validated.
- [ ] Exception mappings verified.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Application.UnitTests/
 │       ├── ErrorObjectTests.cs
 │       ├── ResultTests.cs
 │       └── ResultGenericTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.Application.IntegrationTests/
 │       └── ExceptionMappingPipelineTests.cs
 │
 └── ArchitectureTests/
     ├── DomainLayerDependencyTests.cs
     └── ResultUsageTests.cs
```

## Summary

This test plan validates the canonical Error object model introduced in ADR‑0052.

It ensures that:

- all failures use a standardized Error structure
- Result and `Result<T>` follow a consistent API contract
- exceptions are translated into categorized Error objects
- Domain and Application layers remain properly separated

This ADR establishes the concrete implementation model that underpins the entire Result-based error architecture.
