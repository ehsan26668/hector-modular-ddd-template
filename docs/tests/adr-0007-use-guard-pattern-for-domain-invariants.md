# Test Plan: ADR-0007 Use Guard Pattern for Domain Invariants

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0007](/docs/adr/0007-guard-pattern-for-domain-invariants.md), which adopts the Guard Pattern through a centralized `Ensure` utility class to enforce domain invariants.

This validation is important because invalid state entering the domain model can break aggregate consistency, weaken business rule enforcement, and introduce subtle defects that are difficult to trace. A centralized guard mechanism must therefore remain predictable, expressive, and aligned with the domain exception strategy.

The goal of this plan is to ensure that guard methods consistently protect domain boundaries, throw the correct domain exceptions when rules are violated, and improve readability without weakening invariant enforcement.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on isolated behavior of the `Ensure` guard methods and their interaction with domain exception types.

- **Target Project:**
  - `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

- **Integration Tests:**
  - Focus on verifying that real domain objects use guards correctly in constructors, factory methods, and state-changing behaviors.

- **arget Project:**
  - `tests/IntegrationTests/Hector.Modules.Projects.IntegrationTests`

---

## 1. Scope

### Included

- `Ensure.NotNull`
- `Ensure.NotEmpty`
- `Ensure.NotDefault`
- Guard-driven invariant protection in entities, value objects, and aggregates
- Throwing domain exceptions when validation fails
- Consistent usage of guards at domain boundaries

### Excluded

- Presentation-layer request validation
- FluentValidation or application-layer validation behaviors
- Infrastructure-level data sanitization
- HTTP error mapping concerns

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_ReturnValue_When_NotNullGuardReceivesValidReference

**Scenario:**

- A valid reference passed to `Ensure.NotNull` must be accepted without throwing an exception.

**Arrange:**

- Create a valid non-null reference value.

**Act:**

- Pass the value to `Ensure.NotNull`.

**Assert:**

- No exception is thrown.
- The returned or preserved value remains unchanged if the API returns it.

---

### TC-02

- #### Should_ThrowDomainException_When_NotNullGuardReceivesNull

**Scenario:**

- A null reference must be rejected at the domain boundary.

**Arrange:**

- Define a null reference.

**Act:**

- Pass the null value to `Ensure.NotNull`.

**Assert:**

- A domain exception is thrown.
- The exception message identifies the invalid parameter or invariant clearly.

---

### TC-03

- #### Should_ThrowDomainException_When_NotEmptyGuardReceivesEmptyString

**Scenario:**

- An empty string must not be accepted where domain state requires meaningful text.

**Arrange:**

- Define an empty string value.

**Act:**

- Pass the value to `Ensure.NotEmpty`.

**Assert:**

- A domain exception is thrown.
- The exception clearly indicates that empty values are invalid.

---

### TC-04

- #### Should_ThrowDomainException_When_NotEmptyGuardReceivesWhitespaceOnlyString

**Scenario:**

- A whitespace-only string must be treated as invalid if the domain rule requires non-empty meaningful text.

**Arrange:**

- Define a whitespace-only string.

**Act:**

- Pass the value to `Ensure.NotEmpty`.

**Assert:**

- A domain exception is thrown.
- The guard behavior is consistent with domain invariant expectations.

---

### TC-05

- #### Should_ThrowDomainException_When_NotDefaultGuardReceivesDefaultValue

**Scenario:**

- Default identifier values must be rejected to prevent invalid identity or state initialization.

**Arrange:**

- Define a default value such as `Guid.Empty` or a default strongly typed identifier where applicable.

**Act:**

- Pass the value to `Ensure.NotDefault`.

**Assert:**

- A domain exception is thrown.
- The exception message identifies the invalid default input.

---

### TC-06

- #### Should_NotThrow_When_NotDefaultGuardReceivesValidValue

**Scenario:**

- A valid non-default value must pass guard validation.

**Arrange:**

- Define a valid non-default identifier or value.

**Act:**

- Pass the value to `Ensure.NotDefault`.

**Assert:**

- No exception is thrown.

---

### TC-07

- #### Should_ProtectAggregateInvariant_When_InvalidInputIsPassedToDomainBehavior

**Scenario:**

- Aggregates must use guards to prevent invalid state transitions.

**Arrange:**

- Create a valid aggregate instance.
- Prepare invalid input for a domain behavior or factory method.

**Act:**

- Invoke the behavior with invalid input.

**Assert:**

- A domain exception is thrown.
- The aggregate state remains unchanged.

---

### TC-08

- #### Should_ProtectEntityOrValueObjectConstruction_When_InvalidArgumentsAreProvided

**Scenario:**

- Domain objects must reject invalid constructor or factory arguments through the centralized guard mechanism.

**Arrange:**

- Prepare invalid constructor arguments such as null, empty, or default values.

**Act:**

- Construct the entity or value object.

**Assert:**

- A domain exception is thrown.
- The invalid object is not created.

---

### TC-09

- #### Should_UseEnsureGuards_When_RealDomainModelIsCreatedInProjectsModule

**Scenario:**

- Real module domain types must rely on the shared guard abstraction rather than duplicating validation logic inconsistently.

**Arrange:**

- Identify target domain types in the Projects module.

**Act:**

- Execute construction or state changes with invalid input.

**Assert:**

- Validation is enforced consistently.
- Failure behavior aligns with Ensure-based domain validation.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that guard failures do not leak infrastructure details or internal technical state.

Messages must remain domain-safe and suitable for upper-layer translation.

### 3.2 Observability & Traceability

Verify that guard-triggered failures are easy to diagnose during debugging and test execution.

The failing parameter or invariant should be clearly identifiable from the exception output.

### 3.3 Contract Stability

Verify that the `Ensure` API remains stable for domain consumers.

Future enhancements must not silently change validation semantics in a way that breaks existing domain behavior.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - `null`
  - `string.Empty`
  - `" "`
  - `Guid.Empty`
  - Valid `Guid`
  - Valid and invalid strongly typed identifiers
  - Valid and invalid project names

- **Expected Outputs:**
  - Domain exceptions for null, empty, whitespace-only, and default values
  - No exception for valid values
  - Domain object state remains valid after rejected operations

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Define failing tests for `Ensure.NotNull`, `Ensure.NotEmpty`, and `Ensure.NotDefault`.
2. **GREEN:** Implement the minimal guard logic and connect failures to the domain exception hierarchy.
3. **REFACTOR:** Improve naming, consistency, and reuse while preserving clear invariant intent and green tests.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Guard failures throw the correct domain exception types.
- [ ] Security/Non-functional points verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Domain.UnitTests/
 │       └── EnsureTests.cs
 └── IntegrationTests/
     └── Hector.Modules.Projects.IntegrationTests/
         └── DomainInvariantGuardTests.cs
```

---

## Summary

This test plan ensures that the Guard Pattern is enforced as a consistent domain protection mechanism. By validating centralized guard behavior, correct exception throwing, and real usage inside domain models, the architecture preserves strong invariants while keeping validation logic readable, reusable, and aligned with DDD principles.

---
