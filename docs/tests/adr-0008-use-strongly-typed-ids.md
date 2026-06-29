# Test Plan: ADR-0008 Use Strongly Typed IDs

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0008](/docs/adr/0008-strongly-typed-ids.md), which introduces Strongly Typed IDs for entity identifiers instead of primitive types such as `Guid`, `int`, or `string`.

This validation is essential because identifier misuse is one of the most common sources of subtle domain bugs in large systems. By modeling identifiers as dedicated domain types, the architecture aims to strengthen compile-time safety, improve domain expressiveness, and eliminate accidental cross-aggregate identifier substitution.

The goal of this plan is to ensure that identifier types are strongly typed, preserve correct equality semantics, wrap underlying primitive values consistently, and are used by entities and aggregates instead of raw primitives.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on Strongly Typed ID behavior, equality semantics, primitive value encapsulation, and misuse prevention at the type level.

- **Target Project:**
  - `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

- **Integration Tests:**
  - Focus on verifying that real domain and persistence components correctly use and preserve strongly typed identifiers.

- **Target Project:**
  - `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

### Included

- Base `StronglyTypedId` abstraction behavior
- Equality semantics of strongly typed identifiers
- Encapsulation of underlying primitive value
- Distinction between different identifier types with the same primitive value
- Use of strongly typed identifiers by entities and aggregates
- Persistence readiness of strongly typed identifiers

### Excluded

- Advanced generic enhancements introduced in later ADRs
- Automatic assembly scanning for identifier registration
- Query-side DTO identifier mapping
- JSON serialization rules unless explicitly part of persistence behavior
- Cross-service contract serialization

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_BeEqual_When_TwoStronglyTypedIdsHaveSameConcreteTypeAndValue

**Scenario:**

- Two identifiers of the same concrete type and same underlying value must be considered equal.

**Arrange:**

- Create two instances of the same identifier type with the same `Guid` value.

**Act:**

- Compare them using equality methods/operators.

**Assert:**

- They are equal.
- Their hash codes are equal.

---

### TC-02

- #### Should_NotBeEqual_When_TwoStronglyTypedIdsHaveDifferentValues

**Scenario:**

- Two identifiers of the same concrete type but different values must not be considered equal.

**Arrange:**

- Create two instances of the same identifier type with different primitive values.

**Act:**

- Compare them.

**Assert:**

- They are not equal.
- Their hash codes are not expected to match semantically.

---

### TC-03

- #### Should_NotBeEqual_When_TwoStronglyTypedIdsHaveSameValueButDifferentConcreteTypes

**Scenario:**

- Two identifiers with the same underlying primitive value but different domain types must not be interchangeable.

**Arrange:**

- Create two different identifier types using the same `Guid` value.

**Act:**

- Compare them through object equality where applicable.

**Assert:**

- They are not equal.
- Type distinction is preserved.

---

### TC-04

- #### Should_ExposeUnderlyingValue_When_StronglyTypedIdIsCreated

**Scenario:**

- A strongly typed identifier must encapsulate and expose its underlying primitive value consistently.

**Arrange:**

- Create a strongly typed identifier with a known primitive value.

**Act:**

- Read the wrapped value.

**Assert:**

- The exposed value matches the original primitive input exactly.

---

### TC-05

- #### Should_NotAllowPrimitiveSubstitution_When_EntityRequiresStronglyTypedId

**Scenario:**

- Entities and aggregates must require strongly typed identifiers rather than primitive values.

**Arrange:**

- Inspect entity constructors, properties, or factory methods.

**Act:**

- Evaluate the identifier types used in domain models.

**Assert:**

- Entity identity is represented by a strongly typed identifier.
- Primitive identifier usage is not used as the domain identity contract.

---

### TC-06

- #### Should_BeUsableInCollections_When_StronglyTypedIdsAreUsedAsKeys

**Scenario:**

- Strongly typed identifiers must behave correctly in hash-based collections such as dictionaries and sets.

**Arrange:**

- Create equal and non-equal strongly typed identifier instances.

**Act:**

- Insert them into a hash-based collection.

**Assert:**

- Equal identifiers behave as the same key.
- Different identifiers remain distinct keys.

---

### TC-07

- #### Should_PersistAndRestoreStronglyTypedId_When_UsedWithEntityFrameworkMapping

**Scenario:**

- A strongly typed identifier must round-trip correctly through the persistence layer.

**Arrange:**

- Create an entity with a strongly typed identifier.
- Persist it using the configured EF Core mapping.

**Act:**

- Reload the entity from persistence.

**Assert:**

- The restored identifier has the same concrete type.
- The restored identifier preserves the original primitive value.

---

### TC-08

- #### Should_UseStronglyTypedIds_When_ProjectsModuleDefinesAggregateIdentity

**Scenario:**

- Real module aggregates must use dedicated identifier types rather than primitives.

**Arrange:**

- Inspect the Projects module aggregate and identifier definitions.

**Act:**

- Evaluate the aggregate identity type.

**Assert:**

- `Project` uses `ProjectId`.
- The identity contract is strongly typed and domain-specific.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that strongly typed identifiers do not expose accidental technical metadata beyond the intended wrapped value.

The identifier abstraction must remain simple, explicit, and domain-focused.

### 3.2 Observability & Traceability

Verify that identifier failures and mismatches are diagnosable during debugging and test execution.

Type mismatch between identifiers should be easy to understand from test results and logs.

### 3.3 Contract Stability

Verify that the `StronglyTypedId` abstraction remains stable for all consuming domain and persistence components.

Future changes must not weaken compile-time safety or alter equality semantics unexpectedly.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - `ProjectId(Guid.Parse("11111111-1111-1111-1111-111111111111"))`
  - `ProjectId(Guid.Parse("22222222-2222-2222-2222-222222222222"))`
  - Another identifier type using `Guid.Parse("11111111-1111-1111-1111-111111111111")`
  - Entities that consume strongly typed identifiers

- **Expected Outputs:**
  - Equality for same type and same value
  - Inequality for same type with different value
  - Inequality for different types with same value
  - Correct persistence round-trip behavior
  - No primitive identity contract in domain entities

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Define failing tests for equality, type distinction, wrapped value access, and domain usage.
2. **GREEN:** Implement the minimal `StronglyTypedId` abstraction and migrate aggregate identities to use it.
3. **REFACTOR:** Improve reuse, naming, and persistence compatibility while preserving type safety and green tests.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Entity identities use strongly typed identifiers instead of primitives.
- [ ] Equality and hash code behavior are verified.
- [ ] Persistence round-trip behavior is verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Domain.UnitTests/
 │       ├── StronglyTypedIdTests.cs
 │       └── StronglyTypedIdArchitectureTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── StronglyTypedIdMappingTests.cs
```

---

## Summary

This test plan ensures that Strongly Typed IDs are enforced as a core domain modeling pattern. By validating type safety, equality semantics, value encapsulation, and persistence compatibility, the architecture reduces identifier misuse and strengthens one of the most important tactical DDD boundaries in the system.

---
