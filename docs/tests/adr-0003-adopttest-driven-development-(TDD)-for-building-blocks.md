# Test Plan: ADR-0003 Adopt Test-Driven Development (TDD) for Building Blocks

## Status

Accepted

## Context

This test plan defines the strategy for validating the foundational abstractions of the Hector framework as per [ADR-0003](/docs/adr/0003-adopt-tdd-for-building-blocks.md). Since Building Blocks are the core dependencies for all feature modules, they require 100% reliability. This plan ensures that every architectural primitive is born from a test and functions as an executable specification.

## Test Strategy

The strategy follows a Pure Unit Testing approach using the Triple-A (Arrange, Act, Assert) pattern.

- **Unit Tests:** High-speed tests targeting isolated components with zero external dependencies.
- **Tools:** xUnit, FluentAssertions, and NSubstitute (if needed for internal abstractions).
- **Target Projects:**

  - `tests/ArchitectureTests/Hector.ArchitectureTests`
  - `tests/UnitTests/Hector.BuildingBlocks.Application.UnitTests`
  - `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

---

## 1. Scope

### Included

- Equality and Identity logic (ValueObjects, Entities, StronglyTypedIds).
- Domain Invariants and Guard Clauses.
- AggregateRoot lifecycle (Domain Event collection and clearing).
- Exception handling and error taxonomy.
- Reusable Application/Persistence abstractions.

### Excluded

- Integration with real databases (covered by Integration Tests).
- Feature-specific business logic (covered in Module Unit Tests).

---

## 2. Test Cases (Executable Specifications)

### TC-01: Should_BeEqual_When_ValueObjectsHaveSameProperties

**Scenario:**

- Validate that Value Objects follow value-based equality rather than reference-based equality.

**Arrange:**

- Create two instances of a ValueObject (e.g., `Money`) with identical values.

**Act:**

- Compare them using the == operator or `.Equals()`.

**Assert:**

- They must be considered equal.

---

### TC-02: Should_ThrowBusinessRuleViolationException_When_GuardClauseIsViolated

**Scenario:**

- Ensure that Guard clauses effectively protect domain invariants.

**Arrange:**

- Setup a domain primitive (e.g., `Entity`) with invalid input.

**Act:**

- Trigger the validation/guard logic.

**Assert:**

- A specific domain exception (defined in [ADR-0006](/docs/adr/0006-domain-exceptions.md)) must be thrown.

---

### TC-03: Should_CaptureDomainEvent_When_StateChangesInAggregate

**Scenario:**

- Verify that AggregateRoots correctly track domain events for the Outbox pattern.

**Arrange:**

- Instantiate an `AggregateRoot`.

**Act:**

- Execute a domain method that raises an event.

**Assert:**

- The internal `DomainEvents` collection must contain the expected event.

---

### TC-04: Should_GenerateUniqueAndCorrectType_When_StronglyTypedIdIsCreated

**Scenario:**

- Ensure that StronglyTypedIds prevent type-safety issues across different entities.

**Arrange:**

- Define two different ID types (e.g., `ProjectId` and `TaskId`).

**Act:**

- Attempt to compare or assign one to another.

**Assert:**

- The compiler or runtime must prevent incorrect usage, and values must be correctly mapped to the underlying `Guid`.

---

## 3. Non-Functional Validation Points

### 3.1 Test Execution Speed

Building Block tests must be extremely fast (milliseconds) to encourage frequent execution during the TDD cycle.

### 3.2 Coverage

Aim for 100% branch coverage for core primitives like `ValueObject.cs` and `Entity.cs.`

---

## Test Data

- **Inputs:** Concrete implementations of abstract Building Blocks (e.g., `TestEntity : Entity`).
- **Expected Outputs:** Validated states, expected exception types, and correct equality results.

---

## 5. TDD Execution Plan (The Workflow)

1. **RED:** Identify a new architectural requirement (e.g., “Entities should support Soft Delete”). Write a test in `Hector.BuildingBlocks.Domain.UnitTests` that fails.
2. **GREEN:**  Implement the minimal logic in `Entity.cs` to satisfy the test.
3. **REFACTOR:** Clean up the implementation, ensure it follows the Clean Code standards, and verify the test remains green.

---

## 6. Exit Criteria

- [ ] 100% of Building Block methods are covered by Unit Tests.
- [ ] All tests follow the `Should_ExpectedBehavior_When_StateUnderTest` naming convention.
- [ ] No Building Block code is committed without its corresponding test file.

---

## 7. Proposed Test File Layout

```text
tests/UnitTests/
 ├── Hector.BuildingBlocks.Domain.UnitTests/
 │   ├── Primitives/
 │   │   ├── ValueObjectTests.cs
 │   │   ├── EntityTests.cs
 │   │   └── AggregateRootTests.cs
 │   └── GuardTests.cs
 └── Hector.BuildingBlocks.Application.UnitTests/
     └── Messaging/
         └── MediatorTests.cs
```

---

## Summary

By adopting TDD for Building Blocks, we ensure that the foundation of Hector is “self-documenting” and “self-verifying.” This eliminates the risk of regressions in the most sensitive parts of the system.

---
