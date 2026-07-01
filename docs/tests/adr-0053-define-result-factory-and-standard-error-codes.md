# Test Plan: ADR-0053 Define Result Factory and Standard Error Codes

## Status

Accepted

## Context

This test plan validates the centralized Error factory pattern and standardized error code conventions introduced in ADR‑0053.

ADR‑0052 introduced the canonical Error object model.

However, without a standardized creation mechanism:

- Error objects could be instantiated directly
- error codes could follow inconsistent patterns
- duplication of error construction logic could occur
- modules could define incompatible approaches

ADR‑0053 introduces:

- a centralized Errors factory
- a standardized error code naming convention
- module-specific error helpers
- validation error naming rules

These constraints must be enforced through automated tests.

## Test Strategy

- **Unit Tests**
  - Validate factory behavior
  - Validate error code format
  - Validate metadata handling
  - Target: `tests/UnitTests/Hector.Application.UnitTests`

- **Integration Tests**
  - Validate handler usage of factory
  - Validate validation pipeline error codes
  - Target: `tests/IntegrationTests/Hector.Application.IntegrationTests`

- **Architecture Tests**
  - Prevent direct instantiation of Error
  - Enforce layer boundaries
  - Enforce naming conventions
  - Target: `tests/ArchitectureTests`

---

## 1. Scope

Included:

- Error factory usage
- Error code naming convention
- Validation error code format
- Metadata support
- Layer dependency enforcement

Excluded:

- HTTP status mapping
- Localization

---

## 2. Test Cases

### TC-01: Should_CreateErrorWithCorrectCategoryUsingFactory

**Scenario:**  
Factory methods must assign correct ErrorCategory.

**Arrange:**

Call:

Errors.NotFound(code, message)

**Act:**

Inspect returned Error.

**Assert:**

Verify:

- Category == NotFound
- Code matches provided code
- Message matches provided message

---

### TC-02: Should_CreateErrorWithMetadataUsingFactory

**Scenario:**  
Factory must support optional metadata.

**Arrange:**

Provide metadata dictionary.

**Act:**

Call Errors.Infrastructure(code, message, metadata).

**Assert:**

Verify metadata preserved and accessible.

---

### TC-03: Should_EnforceErrorCodeFormat

**Scenario:**  
Error codes must follow:

`<Module>.<Category>.<ErrorName>`

**Arrange:**

Generate sample error codes.

**Act:**

Validate using regex pattern.

**Assert:**

Verify codes match pattern:

^[A-Za-z]+\.(Validation|NotFound|Unauthorized|Forbidden|Conflict|BusinessRule|Infrastructure|Unexpected)\.[A-Za-z0-9]+$

---

### TC-04: Should_EnforceValidationErrorCodeFormat

**Scenario:**  
Validation errors must follow:

`<Module>.Validation.<Field>.<Rule>`

**Arrange:**

Provide validation error codes.

**Act:**

Validate format.

**Assert:**

Verify pattern:

^[A-Za-z]+\.Validation\.[A-Za-z]+\.[A-Za-z]+$

---

### TC-05: Should_PreventDirectInstantiationOfError

**Scenario:**  
Application code must not use:

new Error(...)

**Arrange:**

Analyze Application assembly.

**Act:**

Run architecture validation.

**Assert:**

Verify no classes instantiate Error directly.

Only factory methods allowed.

---

### TC-06: Should_AllowModuleSpecificErrorHelpers

**Scenario:**  
Modules may define wrapper helpers.

**Arrange:**

Define ProjectErrors helper.

**Act:**

Call ProjectErrors.NotFound(id).

**Assert:**

Verify returned Error created through Errors factory.

---

### TC-07: Should_EnsureDomainLayerDoesNotReferenceErrorsFactory

**Scenario:**  
Domain must remain independent.

**Arrange:**

Analyze Domain assembly.

**Act:**

Run architecture validation.

**Assert:**

Verify Domain does not reference:

- Error
- Errors

---

### TC-08: Should_EnsureHandlersUseFactoryNotConstructor

**Scenario:**  
Handlers must not construct Error manually.

**Arrange:**

Analyze handler implementations.

**Act:**

Run architecture validation.

**Assert:**

Verify handlers do not use:

new Error(

---

### TC-09: Should_PreserveStableErrorCodes

**Scenario:**  
Error codes must be stable identifiers.

**Arrange:**

Define known error codes.

**Act:**

Execute regression validation.

**Assert:**

Verify no error code string changes unexpectedly.

(Compare against approved snapshot list.)

---

### TC-10: Should_IncludeValidationMetadataFields

**Scenario:**  
Validation metadata must include:

- field
- validationRule
- attemptedValue (optional)

**Arrange:**

Simulate validation failure.

**Act:**

Execute validation pipeline.

**Assert:**

Verify metadata contains required keys.

---

## 3. Non-Functional Validation Points

### 3.1 Consistency

- All application failures use Errors factory.
- Error codes follow defined naming convention.

### 3.2 Observability

- Metadata improves structured logging.
- Codes are telemetry-safe.

### 3.3 Architectural Integrity

- Domain independent from error infrastructure.
- Handlers remain clean and expressive.

---

## 4. Test Data

Inputs:

- Sample error codes
- Validation failures
- Metadata dictionaries
- Handler implementations

Expected Outputs:

- Correct ErrorCategory assignment
- Convention-compliant error codes
- No direct Error instantiation
- Preserved metadata

---

## 5. TDD Execution Plan

1. RED

- Write failing architecture test preventing new Error(...)
- Write failing regex validation tests
- Write failing factory category tests

1. GREEN

- Implement Errors factory
- Implement error code validation
- Refactor handlers

1. REFACTOR

- Introduce module-specific helpers
- Remove duplicated code creation

---

## 6. Exit Criteria

- [ ] No direct Error instantiation exists
- [ ] All error codes match convention
- [ ] Domain has zero dependency on Error/Errors
- [ ] Validation errors follow required format
- [ ] Factory assigns correct categories
- [ ] Snapshot of stable error codes approved

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Application.UnitTests/
 │       ├── ErrorFactoryTests.cs
 │       ├── ErrorCodeConventionTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.Application.IntegrationTests/
 │       └── ErrorUsagePipelineTests.cs
 │
 └── ArchitectureTests/
     ├── PreventDirectErrorInstantiationTests.cs
     ├── DomainLayerErrorIsolationTests.cs
     └── ErrorCodeConventionArchitectureTests.cs
```

## Summary

This test plan validates ADR‑0053.

It ensures that:

- Error objects are created through a centralized factory
- Error codes follow a strict naming convention
- Direct instantiation of Error is prevented
- Domain layer remains isolated
- Validation errors follow structured naming rules
- Error codes remain stable across releases

ADR‑0053 strengthens governance and consistency on top of the Error model introduced in ADR‑0052.
