# Test Plan: ADR-0043 Introduce Query Side for CQRS Read Models

## Status

Accepted

## Context

This test plan validates the **Query Side strategy for CQRS Read Models** described in [ADR-0043](/docs/adr/0043-introduce-query-side-for-cqrs-read-models.md).

This ADR standardizes how queries are implemented inside each module. The goal is to ensure that read operations:

- bypass the domain model
- use EF Core projections directly
- return DTO read models
- do not modify state
- remain inside the Application layer

Without strict validation, modules may:

- expose aggregates in read responses
- use domain repositories inside queries
- load full aggregates unnecessarily
- leak domain entities into API contracts
- violate CQRS separation

Because production systems are typically read-heavy, enforcing a clean query-side architecture is critical for performance, maintainability, and modular consistency.

This test plan ensures that the read side is implemented according to the architectural constraints defined in ADR-0043.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Validate handler behavior, projection logic, and immutability guarantees.
  - Target Project: `tests/UnitTests/Hector.Modules.Projects.UnitTests`

- **Integration Tests:**
  - Validate EF Core projection behavior and database interaction.
  - Target Project: `tests/IntegrationTests/Hector.Modules.Projects.IntegrationTests`

- **Architecture Tests (NetArchTest):**
  - Enforce structural rules (no Domain references, no repositories, layer boundaries).
  - Target Project: `tests/ArchitectureTests`

---

## 1. Scope

- **Included:**
  - Query handler location and structure
  - Direct DbContext access from Application layer
  - DTO projection correctness
  - Ensuring no aggregate loading
  - Ensuring no repository usage
  - Ensuring queries do not modify state
  - Layer isolation enforcement

- **Excluded:**
  - Command handling
  - Domain event emission
  - Outbox/Inbox behavior
  - Transactional write flows

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_ReturnProjectedDto_When_QueryIsExecuted

**Scenario:**  
A query is executed and must return a projected DTO using EF Core.

**Arrange:**

- Seed test database with a Project entity.
- Create `GetProjectByIdQuery`.

**Act:**

- Execute the query handler.

**Assert:**

- Verify a `ProjectDto` is returned.
- Verify only required fields are populated.
- Verify no domain entity is returned.

---

### TC-02: Should_NotLoadAggregate_When_ExecutingQuery

**Scenario:**  
Query handlers must not load aggregates or use domain repositories.

**Arrange:**

- Identify query handler type.

**Act:**

- Inspect dependencies via reflection or architecture rules.

**Assert:**

- Verify handler does not depend on `IRepository<>`.
- Verify handler does not depend on aggregate root types.
- Verify handler only depends on DbContext.

---

### TC-03: Should_NotModifyState_When_QueryIsExecuted

**Scenario:**  
Queries must be read-only.

**Arrange:**

- Seed database state.
- Capture current change tracker state.

**Act:**

- Execute query handler.

**Assert:**

- Verify no entities are marked as Modified/Added/Deleted.
- Verify SaveChanges is never called.
- Verify database state remains unchanged.

---

### TC-04: Should_ResideInApplicationLayer_When_QueryHandlerIsDefined

**Scenario:**  
Query handlers must remain inside the Application layer.

**Arrange:**

- Load assembly metadata.

**Act:**

- Inspect namespace and project structure.

**Assert:**

- Verify handlers are located under:
  Modules.ModuleName.Application.Queries
- Verify no query handler exists in Domain layer.

---

### TC-05: Should_ReturnDtoType_NotDomainEntity

**Scenario:**  
Queries must return DTO read models.

**Arrange:**

- Inspect query handler generic return type.

**Act:**

- Extract `TResponse` from `IQueryHandler<TQuery, TResponse>`.

**Assert:**

- Verify response type is not part of Domain assembly.
- Verify response type is defined inside Application layer.
- Verify response type is immutable (record or readonly properties).

---

### TC-06: Should_UseProjection_NotMaterializeEntireEntity

**Scenario:**  
Query should use EF projection (`Select`) instead of loading full entity.

**Arrange:**

- Inspect query handler implementation.

**Act:**

- Execute handler and analyze generated SQL (integration test).

**Assert:**

- Verify SQL selects only required columns.
- Verify no Include statements for aggregate navigation loading.
- Verify projection occurs before materialization.

---

### TC-07: Should_HandleMissingEntity_Gracefully

**Scenario:**  
Query is executed for a non-existing entity.

**Arrange:**

- Ensure database does not contain entity with given ID.

**Act:**

- Execute query handler.

**Assert:**

- Verify controlled exception or null handling according to module convention.
- Verify no unhandled runtime exception leaks.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Encapsulation

- Verify domain entities are never exposed through read models.
- Verify no internal fields (e.g., concurrency tokens) are exposed in DTOs.

### 3.2 Performance

- Verify projection reduces selected columns.
- Verify no unnecessary tracking is enabled (`AsNoTracking` recommended).
- Verify no aggregate graph loading occurs.

### 3.3 Architectural Consistency

- Verify all modules follow the same query structure.
- Verify no module bypasses the Application layer for reads.

---

## 4. Test Data

- **Inputs:**
  - Existing Project entity
  - Non-existing ProjectId
  - Query instance with valid identifier

- **Expected Outputs:**
  - ProjectDto instance
  - Null or controlled error for missing entity
  - No state mutation
  - No domain entity leakage

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED**
   - Write failing test for returning DTO.
   - Write architecture test enforcing no Domain dependency.
   - Write test asserting no state modification.

2. **GREEN**
   - Implement query handler using EF projection.
   - Return DTO record.
   - Ensure direct DbContext usage.

3. **REFACTOR**
   - Extract shared projection patterns if needed.
   - Add `AsNoTracking` for performance optimization.
   - Improve DTO immutability.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Architecture Tests pass.
- [ ] No Domain entity leakage detected.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Modules.Projects.UnitTests/
 │       └── Application/
 │           └── Queries/
 │               └── GetProjectByIdQueryHandlerTests.cs
 ├── IntegrationTests/
 │   └── Hector.Modules.Projects.IntegrationTests/
 │       └── Queries/
 │           └── GetProjectByIdQueryIntegrationTests.cs
 └── ArchitectureTests/
     └── QueryArchitectureTests.cs
```

## Summary

This test plan ensures that ADR-0043 enforces a strict separation between write and read models in the modular DDD architecture.

It guarantees:

- clean CQRS separation
- high-performance read models
- zero domain leakage
- consistent implementation across modules
- structural enforcement through architecture tests

The result is a robust, maintainable, and performance-oriented query side aligned with CQRS principles.
