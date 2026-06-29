# Test Plan: ADR-0018 Domain Identity Generation Policy

## Status

Accepted

## Context

This test plan validates the architectural decision defined in

[ADR‑0018](/docs/adr/0018-domain-identity-generation-policy.md): *Domain Identity Generation Policy*.

The decision standardizes how domain entity identifiers are created in the domain layer when using strongly typed IDs. It prohibits direct use of `Guid.NewGuid()` inside domain code and requires domain-friendly creation through the strongly typed ID factory method, such as `ProjectId.New()`.

This policy is critical because it preserves domain consistency, prevents accidental bypass of identity conventions, and protects the domain layer from infrastructure-level concerns. It also reduces architectural drift by making identifier creation explicit and enforceable.

This test plan verifies that identity generation rules are consistently applied across domain code and that prohibited patterns are detected by architecture tests.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on strongly typed ID behavior, factory semantics, and valid identity creation patterns.

  - **Target Project:** `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

- **Integration Tests:**
  - Focus on architecture-level enforcement that domain assemblies do not directly invoke `Guid.NewGuid()`.

  - **Target Project:** `tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

List exactly what is included and excluded from this test plan to set clear boundaries for the validation process.

### Included

- Strongly typed ID creation through the domain factory method
- Validation that domain entities use standardized identity creation
- Detection of direct `Guid.NewGuid()` usage in domain assemblies
- Enforcement of identity generation conventions through architecture tests

### Excluded

- Persistence-level identity materialization
- Database-generated identifiers
- Infrastructure reconstruction of existing IDs
- External serialization concerns

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_CreateStronglyTypedId_When_DomainUsesFactoryMethod

**Scenario:**

- A strongly typed ID must be created through the intended domain factory method.

**Arrange:**

- Prepare a strongly typed ID type such as `ProjectId`.
- Define the domain creation path.

**Act:**

- Call `ProjectId.New()`.

**Assert:**

- A valid strongly typed ID is returned.
- The generated value is non-empty and usable by the domain.

---

### TC-02

- #### Should_NotAllowDirectGuidGeneration_When_ValidatingDomainIdentityPattern

**Scenario:**

- Domain code must not generate identifiers using `Guid.NewGuid()` directly.

**Arrange:**

- Load the domain assembly.
- Inspect domain types that create entity identifiers.

**Act:**

- Execute architecture analysis against the domain layer.

**Assert:**

- No domain type directly invokes `Guid.NewGuid()`.

---

### TC-03

- #### Should_UseStandardIdentityCreation_When_CreatingNewAggregateInstances

**Scenario:**

- New aggregates must follow the standardized identity creation pattern.

**Arrange:**

- Create a new aggregate instance in the domain layer.

**Act:**

- Instantiate the aggregate through its domain constructor or factory.

**Assert:**

- The aggregate identity is created through the strongly typed ID factory method.
- No raw `Guid` generation is used.

---

### TC-04

- #### Should_AllowInfrastructureToReconstructExistingIds_When_MaterializingPersistedEntities

**Scenario:**

- Infrastructure code may reconstruct existing identifiers from stored values.

**Arrange:**

- Simulate persistence reconstruction using an existing identifier value.

**Act:**

- Materialize the strongly typed ID from the stored value.

**Assert:**

- Reconstruction succeeds.
- The rule against domain-side `Guid.NewGuid()` is not violated.

---

### TC-05

- #### Should_FailArchitectureTest_When_DomainCallsGuidNewGuidDirectly

**Scenario:**

- Architecture tests must detect prohibited identifier generation in the domain layer.

**Arrange:**

- Create or inspect a domain type that calls `Guid.NewGuid()` directly.

**Act:**

- Run the architecture test suite.

**Assert:**

- The test fails.
- The violation is reported clearly.

---

### TC-06

- #### Should_EnforceIdentityConvention_When_ReviewingDomainAssemblies

**Scenario:**

- All domain assemblies must conform to the same identity creation policy.

**Arrange:**

- Load all domain assemblies in the solution.

**Act:**

- Run architecture verification over the assemblies.

**Assert:**

- All domain assemblies comply with the policy.
- No assembly bypasses the convention.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that identity generation does not expose internal persistence details or implementation-specific identifier sources.

### 3.2 Observability & Traceability

Verify that generated identifiers remain stable and traceable across domain events, logs, and persistence operations.

### 3.3 Contract Stability

Verify that strongly typed ID creation contracts remain predictable and consistent for domain consumers.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - `ProjectId.New()`
  - domain aggregate creation
  - architecture test fixtures
  - reconstructed identifier values

- **Expected Outputs:**
  - valid strongly typed IDs
  - no direct `Guid.NewGuid()` usage in domain assemblies
  - architecture test failure for violations

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Define failing architecture tests that detect direct `Guid.NewGuid()` usage in domain assemblies.

2. **GREEN:**  
   Implement or adjust domain identity creation to use strongly typed ID factory methods.

3. **REFACTOR:**  
   Consolidate identity creation patterns and keep enforcement rules maintainable as new modules are added.

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
 │       └── StronglyTypedIds/
 │           └── StronglyTypedIdCreationTests.cs
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         └── DomainIdentityTests.cs
```

---

## Summary

This test plan validates the domain identity generation policy introduced by [ADR‑0018](/docs/adr/0018-domain-identity-generation-policy.md). The tests ensure that domain code creates identifiers through the approved strongly typed ID factory methods and that direct `Guid.NewGuid()` usage in the domain layer is blocked by architecture tests. This preserves consistency, reinforces DDD boundaries, and reduces long-term architectural drift.

---
