# Test Plan: ADR-0009 Strongly Typed IDs Enhancement

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0009](/docs/adr/0009-strongly-typed-ids-enhancement.md), which enhances the `StronglyTypedId` abstraction introduced in [ADR-0008](/docs/adr/0008-strongly-typed-ids.md).

The previous decision established type-safe identifiers instead of primitive identity values. This enhancement extends that model by introducing a standardized ID creation mechanism, especially for `Guid`-based identifiers, and by improving developer ergonomics while preserving persistence compatibility.

This validation is important because identifier generation must remain consistent, explicit, and infrastructure-safe. If generation behavior becomes fragmented across modules, the domain model can drift into inconsistent identity creation patterns. The goal of this plan is to ensure that the enhanced abstraction reduces boilerplate without weakening type safety, equality semantics, or EF Core compatibility.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on the enhanced `StronglyTypedId` abstraction, ID generation semantics, equality behavior, and developer-facing factory APIs.

- **Target Project:**
  - `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

- **Integration Tests:**
  - Focus on verifying that generated strongly typed identifiers remain compatible with EF Core persistence and module-level usage.

- **arget Project:**
  - `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

- **Module Integration Tests:**
  - Tests: Focus on verifying that real module aggregates consume the enhanced identifier pattern consistently.

- **Target Project:**
  - `tests/IntegrationTests/Hector.Modules.Projects.IntegrationTests`

---

## 1. Scope

### Included

- Standardized creation of new strongly typed identifiers
- `New()`-style generation behavior for `Guid`-based IDs
- Parameterless or simplified factory creation patterns where applicable
- Equality behavior of generated identifiers
- Continued encapsulation of underlying primitive values
- EF Core value conversion compatibility after enhancement
- Real usage in module aggregates such as `ProjectId`

### Excluded

- Serialization enhancements introduced by later ADRs
- Assembly scanning and auto-registration introduced in later ADRs
- Cross-process identifier transport concerns
- Non-`Guid` specialized generation strategies unless explicitly supported by the base - abstraction
- Distributed ID generation concerns

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_CreateNewInstance_When_NewFactoryMethodIsCalledForGuidBasedId

**Scenario:**

- A `Guid`-based strongly typed identifier must support a standard creation mechanism through a `New()`-style API.

**Arrange:**

- Select a concrete `Guid`-based strongly typed ID type such as `ProjectId`.

**Act:**

- Call the standard factory method for new identifier creation.

**Assert:**

- A new strongly typed identifier instance is returned.
- The underlying value is not `Guid.Empty`.

---

### TC-02

- #### Should_CreateDistinctValues_When_NewFactoryMethodIsCalledMultipleTimes

**Scenario:**

- Repeated calls to the standard ID generation API must produce distinct identities.

**Arrange:**

- Select a concrete strongly typed identifier type.

**Act:**

- Call the generation API multiple times.

**Assert:**

- Returned identifiers are not equal.
- Their wrapped primitive values are distinct.

---

### TC-03

- #### Should_PreserveEqualitySemantics_When_TwoIdsWrapSameGeneratedValue

**Scenario:**

- The enhancement must not change the equality contract established in [ADR-0008](/docs/adr/0008-strongly-typed-ids.md).

**Arrange:**

- Generate a primitive value once.
- Create two instances of the same strongly typed ID type using that same value.

**Act:**

- Compare the identifiers.

**Assert:**

- They are equal.
- Their hash codes are equal.

---

### TC-04

- #### Should_NotBeEqual_When_GeneratedIdsHaveDifferentValues

**Scenario:**

- IDs generated separately must remain distinct and non-equal.

**Arrange:**

- Generate two identifiers independently using the standardized creation API.

**Act:**

- Compare them.

**Assert:**

- They are not equal.
- They behave as separate identities.

---

### TC-05

- #### Should_ExposeGeneratedPrimitiveValue_When_IdIsCreatedThroughEnhancedFactory

**Scenario:**

- The enhanced creation API must still produce identifiers whose underlying primitive value can be read consistently.

**Arrange:**

- Create a strongly typed identifier using the enhanced factory method.

**Act:**

- Read the encapsulated primitive value.

**Assert:**

- The value is present and valid.
- For `Guid` identifiers, the value is not `Guid.Empty`.

---

### TC-06

- #### Should_RemainCompatibleWithEfCoreValueConverter_When_EnhancedIdIsPersistedAndRestored

**Scenario:**

- The enhanced base class must remain persistence-safe and compatible with EF Core Value Converters.

**Arrange:**

- Create an entity using an identifier produced through the enhanced creation mechanism.
- Persist the entity through EF Core.

**Act:**

- Reload the entity from the database.

**Assert:**

- The identifier round-trips successfully.
- The restored identifier has the same concrete type and underlying value.

---

### TC-07

- #### Should_UseEnhancedIdentifierCreation_When_ProjectAggregateIsCreated

**Scenario:**

- Real module aggregates should use the enhanced identifier creation pattern instead of ad hoc primitive generation.

**Arrange:**

- Identify aggregate creation flow in the Projects module.

**Act:**

- Create a new `Project` aggregate through its public domain API.

**Assert:**

- The aggregate identity is a `ProjectId`.
- The identifier is valid and not default.
- Identity creation follows the centralized strongly typed ID pattern.

---

### TC-08

- #### Should_NotRequirePrimitiveGuidGenerationLogicInsideDomainAggregate_When_EnhancedIdFactoryExists

**Scenario:**

- Aggregate code should not need to manually call primitive identity generation logic if the strongly typed ID abstraction provides it.

**Arrange:**

- Inspect the aggregate creation pattern in domain code.

**Act:**

- Evaluate how IDs are created in the aggregate factory or constructor path.

**Assert:**

- Identity creation is delegated to the strongly typed ID type or shared abstraction.
- Primitive generation logic is not duplicated across aggregates.

---

### TC-09

- #### Should_SupportHashBasedCollections_When_IdsAreGeneratedThroughEnhancedFactory

**Scenario:**

- Generated strongly typed identifiers must still behave correctly in hash-based collections.

**Arrange:**

- Create identifiers through the enhanced generation API.

**Act:**

- Use them in a dictionary or hash set.

**Assert:**

- Equality and hash code behavior remain correct.
- Collection semantics are preserved.

---

## 3. Non-Functional Validation Points

### 3.1 Developer Experience

Verify that the enhancement reduces boilerplate for domain developers.

Creating a new identifier should be straightforward, discoverable, and consistent across modules.

### 3.2 Contract Stability

Verify that the enhancement extends the original `StronglyTypedId` abstraction without breaking ADR-0008 expectations.

Previously valid equality and persistence semantics must remain unchanged.

### 3.3 Persistence Safety

Verify that the additional generation and factory behavior does not introduce incompatibility with EF Core materialization or value conversion patterns.

### 3.4 Domain Consistency

Verify that identity generation remains centralized and predictable rather than being scattered across aggregate implementations.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - `ProjectId.New()`
  - Two consecutive calls to `ProjectId.New()`
  - Two `ProjectId` instances created from the same known `Guid`
  - Persisted entity using enhanced strongly typed identifier

- **Expected Outputs:**
  - Newly generated IDs are valid and non-default
  - Consecutively generated IDs are distinct
  - Same type plus same value results in equality
  - EF Core round-trip preserves identity type and value
  - Aggregate creation uses centralized ID generation

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Define failing tests for `New()` generation, distinctness, preserved equality, and EF Core round-trip compatibility.
2. **GREEN:** Implement the minimal enhancement to the `StronglyTypedId` abstraction and update concrete identifier types to use the new factory pattern.
3. **REFACTOR:** Remove duplicated primitive generation logic from aggregates and improve consistency without changing observable behavior.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Generated strongly typed IDs are valid and non-default.
- [ ] Equality semantics from [ADR-0008](/docs/adr/0008-strongly-typed-ids.md) remain preserved.
- [ ] EF Core persistence compatibility is verified.
- [ ] Aggregate identity creation uses the enhanced abstraction consistently.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Domain.UnitTests/
 │       ├── StronglyTypedIdTests.cs
 │       └── StronglyTypedIdArchitectureTests.cs
 ├── IntegrationTests/
 │   └── Hector.BuildingBlocks.Persistence.IntegrationTests/
 │       └── StronglyTypedIdMappingTests.cs
 └────── Hector.Modules.Projects.IntegrationTests/
         └── ProjectsStronglyTypedIdMappingTests.cs
```

---

## Summary

This test plan ensures that the Strongly Typed ID abstraction evolves safely from basic type safety toward a more ergonomic and standardized identity model. By validating centralized ID generation, preserved equality semantics, and continued persistence compatibility, the architecture improves developer experience without compromising DDD boundaries or technical reliability.

---
