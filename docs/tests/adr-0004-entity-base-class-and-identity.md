# Test Plan: ADR-0004 Entity Base Class and Identity

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0004](/docs/adr/0004-entity-base-class-and-identity.md), which standardizes identity handling through a shared `Entity<TId>` base class in the Building Blocks layer.

This validation is critical because entities are one of the core tactical DDD patterns, and incorrect equality semantics can break aggregate behavior, introduce subtle bugs, and create inconsistent domain models across modules.

The goal of this plan is to ensure that all entities consistently follow identity-based equality, enforce type-safe comparisons, and clearly preserve the distinction between Entities and ValueObjects.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:** Focus on isolated entity equality logic, operator behavior, `Equals`, and `GetHashCode` consistency.

- **Target Project:**
  - `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

- **Integration Tests:** Focus on verifying that real module entities inherit from the base abstraction and preserve identity semantics in realistic domain usage.

- **Target Project:**
  - `tests/IntegrationTests/Hector.Modules.Projects.IntegrationTests`

---

## 1. Scope

### Included

- Identity-based equality behavior of `Entity<TId>`
- Equality operator behavior (`==`, `!=`)
- Equals and `GetHashCode` consistency
- Same-type requirement for equality
- Use of strongly typed identifiers as entity identity
- Verification that module entities inherit from `Entity<TId>`

### Excluded

- Persistence mapping of entity identifiers
- Domain event behavior
- ValueObject structural equality rules
- Feature-specific business rules unrelated to identity semantics

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_BeEqual_When_TwoEntitiesHaveSameIdAndConcreteType

**Scenario:**

- Two entity instances with the same strongly typed identifier and the same concrete type must be considered equal.

**Arrange:**

- Create two instances of the same test entity type.
- Assign the same `TId` value to both instances.

**Act:**

- Compare the entities using `Equals` and `==`.

**Assert:**

- Both comparisons return `true`.
- `!=` returns `false`.

---

### TC-02: Should_NotBeEqual_When_TwoEntitiesHaveDifferentIds

**Scenario:**

- Two entities of the same concrete type with different identifiers must not be considered equal.

**Arrange:**

- Create two instances of the same test entity type.
- Assign different strongly typed identifiers.

**Act:**

- Compare the entities using `Equals` and equality operators.

**Assert:**

- `Equals` returns `false`.
- `==` returns `false`.
- `!=` returns `true`.

---

### TC-03: Should_NotBeEqual_When_TwoEntitiesHaveSameIdButDifferentConcreteTypes

**Scenario:**

- Entities with the same identifier value but different concrete types must not be considered equal.

**Arrange:**

- Create two entity instances of different derived types.
- Assign identifiers with the same underlying value where technically possible.

**Act:**

- Compare the entities.

**Assert:**

- `Equals` returns `false`.
- `==` returns `false` if comparison is supported through base abstraction.
- Cross-type identity equivalence is rejected.

---

### TC-04: Should_ReturnSameHashCode_When_TwoEntitiesAreEqual

**Scenario:**

- Equal entities must produce the same hash code to preserve correctness in hash-based collections.

**Arrange:**

- Create two equal entities with the same concrete type and same identifier.

**Act:**

- Call `GetHashCode()` on both instances.

**Assert:**

- Both hash codes are equal.

---

### TC-05: Should_NotBeEqual_When_ComparingEntityWithNull

**Scenario:**

- An entity must never be equal to `null`.

**Arrange:**

- Create a valid entity instance.
- Define a `null` entity reference.

**Act:**

- Compare the entity with `null` using `Equals` and equality operators.

**Assert:**

- Equality returns `false`.
- Inequality returns `true`.

---

### TC-06: Should_InheritFromEntityBaseClass_When_DomainEntityIsDefinedInModule

**Scenario:**

- All domain entities in feature modules must inherit from `Entity<TId>` to comply with the architectural rule.

**Arrange:**

- Identify concrete entity types in module domain assemblies.

**Act:**

- Inspect their inheritance chain.

**Assert:**

- Each entity derives from `Entity<TId>`.

---

### TC-07: Should_UseStronglyTypedId_When_EntityIdentityIsDefined

**Scenario:**

- Entity identifiers must be strongly typed to prevent primitive obsession and unsafe cross-entity comparison.

**Arrange:**

- Inspect entity identity property definitions in test entities and module entities.

**Act:**

- Evaluate the identifier type used by `Entity<TId>`.

**Assert:**

- Identity types are strongly typed identifiers rather than primitives such as `Guid` or `string`.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that identity comparison behavior does not expose internal implementation details outside the domain layer.

No sensitive internal state beyond identity should participate in equality logic.

### 3.2 Observability & Traceability

Verify that identity semantics remain deterministic and traceable during debugging and test execution.

Test failures should clearly reveal whether the mismatch is caused by identifier inequality or concrete type mismatch.

### 3.3 Contract Stability

Verify that the `Entity<TId>` equality contract remains stable for all consuming modules.

Future changes must not alter equality semantics in a way that breaks aggregate consistency or collection behavior.

---

## Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - ProjectId(`Guid.Parse("11111111-1111-1111-1111-111111111111")`)
  - ProjectId(`Guid.Parse("22222222-2222-2222-2222-222222222222")`)
  - Test entities of the same and different concrete types
  - `null` entity reference

- **Expected Outputs:**
  - Equality only for same identifier and same concrete type
  - Inequality for different identifiers
  - Inequality for different concrete types
  - Stable hash code for equal entities

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Define failing tests for equality semantics, null comparison, and concrete type checks.
2. **GREEN:** Implement the minimal `Entity<TId>` logic for Equals, operators, and GetHashCode.
3. **REFACTOR:** Simplify equality implementation, remove duplication, and improve readability while keeping tests green.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Security/Non-functional points verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Domain.UnitTests/
 │       └── Primitives/
 │           └── EntityTests.cs
 └── IntegrationTests/
     └── Hector.Modules.Projects.IntegrationTests/
         └── EntityIdentityIntegrationTests.cs
```

---

## Summary

This test plan ensures that entity identity semantics are enforced consistently across the architecture. By validating equality, type safety, inheritance, and identifier modeling, the system preserves one of the most important DDD distinctions: entities are defined by identity, not by attributes.

---
