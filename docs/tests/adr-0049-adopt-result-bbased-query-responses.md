# Test Plan: ADR-0049 Adopt Result-Based Query Responses

## Status

Accepted

## Context

This test plan validates the adoption of **Result-based response contracts for Query handlers** as defined in [ADR-0049](/docs/adr/0049-adopt-result-based-query-responses.md).

In a CQRS architecture, queries represent read operations.

Historically, query handlers returned raw values such as:

- DTOs
- primitive values
- collections
- nullable responses

However, queries may fail for reasons such as:

- authorization failures
- validation failures
- missing resources
- infrastructure failures
- unexpected exceptions

Since ADR‑0047 standardizes the Result pattern for the Application Layer, query handlers must follow the same model.

All query handlers must return:

`Result<TResponse>`

Queries must not return:

- raw DTOs
- primitive values
- collections
- nullable types

Additionally, ADR‑0049 defines specific semantics for NotFound handling and restricts allowed failure categories (further formalized in ADR‑0051).

This test plan ensures correct contract enforcement, failure semantics, and exception translation.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests**
  - Validate Result-based query responses.
  - Validate NotFound semantics.
  - Target Project: `tests/UnitTests/Hector.Application.UnitTests`

- **Integration Tests**
  - Validate query execution through the application pipeline.
  - Validate exception translation and failure categories.
  - Target Project: `tests/IntegrationTests/Hector.Application.IntegrationTests`

- **Architecture Tests**
  - Enforce `Result<TResponse>` return types.
  - Prevent raw return types in query handlers.
  - Target Project: `tests/ArchitectureTests`

---

## 1. Scope

- **Included:**
  - Query handler return type enforcement
  - NotFound policy validation
  - Allowed query failure categories
  - Infrastructure exception translation
  - Result-based error handling consistency

- **Excluded:**
  - Command failure policies
  - Validation pipeline mechanics (ADR‑0048)
  - Error taxonomy structure (ADR‑0050)

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_ReturnResultOfResponseType_ForAllQueryHandlers

**Scenario:**  
All query handlers must return `Result<TResponse>`.

**Arrange:**

Scan Application assemblies for query handlers.

**Act:**

Run architecture validation tests.

**Assert:**

- Verify all query handlers return `Result<TResponse>`.
- Verify no handler returns raw DTOs.
- Verify no handler returns primitive or nullable types.

---

### TC-02: Should_ReturnFailure_When_SingleRequiredResourceNotFound

**Scenario:**  
A required single resource is not found.

Example: GetProjectById

**Arrange:**

Create a query for a non-existing entity.

**Act:**

Execute the query.

**Assert:**

- Verify response is `Result.Failure`.
- Verify failure category is `NotFound`.
- Verify no exception is thrown.

---

### TC-03: Should_ReturnSuccessWithEmptyCollection_When_CollectionQueryHasNoResults

**Scenario:**  
A collection query returns no data.

Example: ListProjects

**Arrange:**

Ensure no entities exist.

**Act:**

Execute the query.

**Assert:**

- Verify response is `Result.Success`.
- Verify value is an empty collection.
- Verify no NotFound failure is returned.

---

### TC-04: Should_ReturnSuccessWithNull_When_OptionalResourceIsNotFound

**Scenario:**  
An optional resource query returns no result.

Example: FindProjectBySlug

**Arrange:**

Execute query for a non-existing optional resource.

**Act:**

Execute the query.

**Assert:**

- Verify response is `Result.Success`.
- Verify value is null.
- Verify no failure is returned.

---

### TC-05: Should_TranslateInfrastructureExceptions_ToFailureResult

**Scenario:**  
Infrastructure exceptions occur during query execution.

Examples:

- SqlException
- TimeoutException
- NullReferenceException

**Arrange:**

Simulate infrastructure exception in repository.

**Act:**

Execute the query.

**Assert:**

- Verify exception does not propagate.
- Verify response is `Result.Failure`.
- Verify failure category is `Infrastructure` or `Unexpected`.

---

### TC-06: Should_AllowOnlyPermittedFailureCategories_ForQueries

**Scenario:**  
Query failures must belong to allowed categories.

Allowed categories:

- Validation
- NotFound
- Unauthorized
- Forbidden
- Infrastructure
- Unexpected

**Arrange:**

Analyze query handler implementations.

**Act:**

Run architecture validation tests.

**Assert:**

- Verify queries do not produce:
  - BusinessRule
  - Conflict
- Verify failure categories comply with ADR‑0051 constraints.

---

### TC-07: Should_PreventPrimitiveOrCollectionReturnTypes_InQueryHandlers

**Scenario:**  
Query handlers must not return primitive or collection types directly.

**Arrange:**

Scan Application assembly for query handlers.

**Act:**

Run architecture enforcement tests.

**Assert:**

- Verify no handler returns:
  - `Task<List<T>>`
  - `Task<IEnumerable<T>>`
  - `Task<Guid>`
  - `Task<bool>`
  - `Task<int>`
  - `Task<T?>`
- Verify all return types are `Result<TResponse>`.

---

### TC-08: Should_EnsureUnauthorizedOrForbiddenFailures_ReturnResultFailure

**Scenario:**  
Authorization failure occurs during query execution.

**Arrange:**

Simulate unauthorized access.

**Act:**

Execute the query.

**Assert:**

- Verify response is `Result.Failure`.
- Verify error category is `Unauthorized` or `Forbidden`.
- Verify no exception leaks outside Application Layer.

---

## 3. Non-Functional Validation Points

### 3.1 Consistency

- Verify all queries follow the same Result-based contract.
- Verify NotFound semantics remain consistent across modules.

### 3.2 Observability

- Verify query failures provide structured error metadata.
- Verify infrastructure failures are traceable.

### 3.3 Architectural Integrity

- Verify Application Layer absorbs infrastructure exceptions.
- Verify query failure categories comply with ADR‑0051.

---

## 4. Test Data

- **Inputs:**
  - Existing resource query
  - Non-existing required resource
  - Non-existing optional resource
  - Empty dataset
  - Simulated infrastructure failure
  - Unauthorized access

- **Expected Outputs:**
  - Result.Success(value)
  - Result.Success(empty collection)
  - Result.Success(null)
  - Result.Failure(NotFound)
  - Result.Failure(Unauthorized)
  - Result.Failure(Infrastructure)
  - Result.Failure(Unexpected)

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. RED

Write failing tests for:

- query handler return type enforcement
- NotFound policy enforcement
- infrastructure exception translation
- failure category restrictions

1. GREEN

Implement:

- `Result<TResponse>` return contracts
- standardized NotFound handling
- exception translation logic

1. REFACTOR

Simplify query handlers and ensure consistent failure handling across all modules.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Architecture tests enforce `Result<TResponse>` contract.
- [ ] NotFound semantics validated.
- [ ] Infrastructure exceptions do not escape Application Layer.
- [ ] Failure categories comply with ADR‑0051.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Application.UnitTests/
 │       ├── QueryResultTests.cs
 │       └── QueryNotFoundPolicyTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.Application.IntegrationTests/
 │       └── QueryPipelineTests.cs
 │
 └── ArchitectureTests/
     ├── QueryHandlerReturnTypeTests.cs
     └── QueryFailureCategoryTests.cs
```

## Summary

This test plan validates the adoption of Result-based Query Responses defined in ADR‑0049.

It ensures that:

- all query handlers return `Result<TResponse>`
- NotFound semantics are applied consistently
- infrastructure exceptions are translated into Result failures
- only permitted failure categories are used
- raw return types are eliminated from the Application Layer

This standardizes the Query side of CQRS and aligns it with the unified Result-based Application contract introduced in ADR‑0047.
