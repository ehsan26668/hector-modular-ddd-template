# Test Plan: ADR-0050 Establish Application Error Taxonomy

## Status

Accepted

## Context

This test plan validates the centralized Application Error Taxonomy defined in [ADR-0050](/docs/adr/0050-establish-application-error-taxonomy.md).

As the Application Layer adopts the Result pattern for commands and queries, failures must follow a consistent and enforceable classification model.

Without a standardized taxonomy:

- modules may define inconsistent error semantics
- HTTP mappings become unreliable
- observability becomes fragmented
- automated architecture validation becomes impossible
- client contracts become unstable over time

ADR‑0050 establishes:

- a fixed set of allowed error categories
- centralized error catalogs
- stable error code conventions
- exception-to-error mappings
- HTTP mapping policies
- architecture enforcement requirements

This test plan ensures error classification consistency, contract stability, and architecture compliance across the entire Application Layer.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests**
  - Validate Error creation behavior.
  - Validate category assignment and metadata.
  - Validate exception-to-error mappings.
  - Target Project: `tests/UnitTests/Hector.Application.UnitTests`

- **Integration Tests**
  - Validate exception translation through application execution flow.
  - Validate consistent Result failure generation.
  - Validate Web-layer HTTP mappings.
  - Target Project: `tests/IntegrationTests/Hector.Application.IntegrationTests`

- **Architecture Tests**
  - Enforce taxonomy rules and catalog structure.
  - Validate error code uniqueness and naming conventions.
  - Validate layer boundaries.
  - Target Project: `tests/ArchitectureTests`

- **Contract / Snapshot Tests**
  - Protect error contract stability over time.
  - Detect accidental error code changes.
  - Target Project: `tests/ContractTests`

---

## 1. Scope

- **Included:**
  - Error category enforcement
  - Centralized error catalog enforcement
  - Error code naming conventions
  - Error code uniqueness
  - Exception mapping validation
  - HTTP status mapping validation
  - Contract stability verification
  - Layer dependency validation

- **Excluded:**
  - Business workflow correctness
  - Domain model validation rules
  - UI error rendering

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_AllowOnlyDefinedErrorCategories

**Scenario:**  
Only approved error categories may exist in the system.

**Arrange:**

Load all declared ErrorCategory values.

**Act:**

Run architecture validation.

**Assert:**

Verify only the following categories exist:

- Validation
- NotFound
- Unauthorized
- Forbidden
- Conflict
- BusinessRule
- Infrastructure
- Unexpected

Verify no custom categories are introduced by modules.

---

### TC-02: Should_EnsureEveryFailureContainsCategory

**Scenario:**  
Every Result failure must include a categorized Error.

**Arrange:**

Create failure Results across commands and queries.

**Act:**

Inspect generated Error objects.

**Assert:**

- Verify category is always assigned.
- Verify category is never null or undefined.

---

### TC-03: Should_EnsureAllErrorCodesAreUnique

**Scenario:**  
All Error.Code values must be globally unique.

**Arrange:**

Scan all centralized error catalogs.

**Act:**

Collect all Error.Code values.

**Assert:**

- Verify no duplicate error codes exist.
- Verify uniqueness across all modules.

---

### TC-04: Should_EnsureAllErrorCodesFollowNamingConvention

**Scenario:**  
All error codes must follow the standard naming convention.

Pattern:

^[A-Z]+(_[A-Z]+)+$

**Arrange:**

Load all declared error codes.

**Act:**

Validate using regex.

**Assert:**

Verify all codes match:

MODULE_REASON

Examples:

- PROJECT_NOT_FOUND
- USER_EMAIL_INVALID
- AUTHENTICATION_REQUIRED

---

### TC-05: Should_EnsureErrorsAreDeclaredInCentralizedCatalogs

**Scenario:**  
Errors must not be created ad-hoc.

**Arrange:**

Analyze Application Layer source code.

**Act:**

Run architecture tests.

**Assert:**

Verify errors are declared only inside:

- Errors.Commands.*
- Errors.Queries.*
- Errors.Shared.*

Verify handlers, validators, and services do not instantiate ad-hoc Error objects.

---

### TC-06: Should_EnsureCommandErrorsAreDeclaredUnderCommandsCatalog

**Scenario:**  
Command-related errors must exist under Errors.Commands.*

**Arrange:**

Analyze command handlers and referenced errors.

**Act:**

Run architecture validation.

**Assert:**

Verify command handlers reference only:

Errors.Commands.*
or
Errors.Shared.*

---

### TC-07: Should_EnsureQueryErrorsAreDeclaredUnderQueriesCatalog

**Scenario:**  
Query-related errors must exist under Errors.Queries.*

**Arrange:**

Analyze query handlers and referenced errors.

**Act:**

Run architecture validation.

**Assert:**

Verify query handlers reference only:

Errors.Queries.*
or
Errors.Shared.*

---

### TC-08: Should_MapDomainException_ToBusinessRuleCategory

**Scenario:**  
Domain exceptions must translate to BusinessRule failures.

**Arrange:**

Throw DomainException from domain logic.

**Act:**

Execute application flow.

**Assert:**

- Verify exception does not escape Application Layer.
- Verify Result.Failure returned.
- Verify category equals BusinessRule.

---

### TC-09: Should_MapValidationException_ToValidationCategory

**Scenario:**  
Validation exceptions must map to Validation errors.

**Arrange:**

Trigger FluentValidationException.

**Act:**

Execute request pipeline.

**Assert:**

- Verify Result.Failure returned.
- Verify category equals Validation.

---

### TC-10: Should_MapConcurrencyException_ToConflictCategory

**Scenario:**  
Concurrency failures must map to Conflict.

**Arrange:**

Simulate DbUpdateConcurrencyException.

**Act:**

Execute command.

**Assert:**

- Verify Result.Failure returned.
- Verify category equals Conflict.

---

### TC-11: Should_MapInfrastructureFailures_ToInfrastructureCategory

**Scenario:**  
External dependency failures occur.

**Arrange:**

Simulate database/network/external service failure.

**Act:**

Execute request.

**Assert:**

- Verify failure category equals Infrastructure.
- Verify exception does not leak.

---

### TC-12: Should_MapUnhandledExceptions_ToUnexpectedCategory

**Scenario:**  
Unhandled exceptions occur during execution.

**Arrange:**

Throw unexpected runtime exception.

**Act:**

Execute request.

**Assert:**

- Verify Result.Failure returned.
- Verify category equals Unexpected.

---

### TC-13: Should_MapErrorCategoriesToCorrectHttpStatusCodes

**Scenario:**  
Web layer maps categories consistently to HTTP status codes.

**Arrange:**

Create failures for each category.

**Act:**

Execute API request.

**Assert:**

Verify mappings:

- Validation → 400
- NotFound → 404
- Unauthorized → 401
- Forbidden → 403
- Conflict → 409
- BusinessRule → 422
- Infrastructure → 503
- Unexpected → 500

---

### TC-14: Should_EnsureApplicationLayerDoesNotDependOnWebAbstractions

**Scenario:**  
Application Layer must remain independent from ASP.NET Core and HTTP abstractions.

**Arrange:**

Analyze Application assemblies.

**Act:**

Run architecture dependency tests.

**Assert:**

Verify Application Layer does not reference:

- ASP.NET Core
- HttpStatusCode
- ProblemDetails
- IActionResult
- Minimal API result types

---

### TC-15: Should_ProtectErrorContractsUsingSnapshotTests

**Scenario:**  
Error contracts must remain stable after release.

**Arrange:**

Generate snapshot of all Error definitions.

**Act:**

Compare current error catalog against approved baseline.

**Assert:**

- Verify existing error codes are unchanged.
- Verify categories remain stable.
- Verify accidental removals are detected.

---

### TC-16: Should_EnsureErrorCodesAreNotReusedForDifferentSemantics

**Scenario:**  
Error codes must never be reassigned to different meanings.

**Arrange:**

Compare historical snapshots.

**Act:**

Detect semantic changes.

**Assert:**

Verify identical Error.Code values retain the same:

- meaning
- category
- contract semantics

---

## 3. Non-Functional Validation Points

### 3.1 Consistency

- Verify all modules follow the same taxonomy.
- Verify HTTP mappings remain deterministic.

### 3.2 Observability

- Verify failures expose structured and searchable metadata.
- Verify logs and monitoring systems can group errors by category and code.

### 3.3 Contract Stability

- Verify released error codes remain stable over time.
- Verify snapshot tests protect external API consumers.

### 3.4 Architectural Integrity

- Verify centralized error governance.
- Verify Application Layer remains independent from Web concerns.

---

## 4. Test Data

- **Inputs:**
  - Valid Error definitions
  - Invalid naming convention samples
  - Duplicate error codes
  - DomainException
  - ValidationException
  - DbUpdateConcurrencyException
  - Infrastructure failures
  - Unexpected runtime exceptions

- **Expected Outputs:**
  - Categorized Result failures
  - Stable Error contracts
  - Correct HTTP mappings
  - Architecture validation success

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. RED

Write failing tests for:

- error code uniqueness
- naming convention enforcement
- centralized catalog enforcement
- exception mappings
- snapshot contract protection

1. GREEN

Implement:

- Error taxonomy model
- centralized catalog structure
- exception translation logic
- HTTP mapping layer

1. REFACTOR

Simplify catalog organization and improve architecture enforcement utilities.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] All Architecture Tests pass.
- [ ] Error codes are globally unique.
- [ ] Naming convention enforced.
- [ ] Exception mappings validated.
- [ ] HTTP mappings validated.
- [ ] Snapshot stability tests passing.
- [ ] Application Layer independent from Web abstractions.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Application.UnitTests/
 │       ├── ErrorCategoryTests.cs
 │       ├── ErrorCodeConventionTests.cs
 │       └── ExceptionMappingTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.Application.IntegrationTests/
 │       ├── ErrorPipelineTests.cs
 │       └── HttpErrorMappingTests.cs
 │
 ├── ArchitectureTests/
 │   ├── ErrorCatalogStructureTests.cs
 │   ├── ErrorCodeUniquenessTests.cs
 │   ├── ApplicationLayerDependencyTests.cs
 │   └── ErrorCategoryPolicyTests.cs
 │
 └── ContractTests/
     └── ErrorContractSnapshotTests.cs
```

## Summary

This test plan validates the centralized Application Error Taxonomy defined in ADR‑0050.

It ensures that:

- all failures use standardized categories
- errors are centrally governed
- error codes are unique and stable
- exception mappings are consistent
- HTTP mappings remain deterministic
- Application Layer stays independent from Web concerns
- error contracts are protected against accidental breaking changes

This ADR establishes the foundational error governance model for the entire Application Layer.
