# Test Plan: ADR-0010 Advanced Capabilities for Strongly Typed IDs

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0010](/docs/adr/0010-advanced-strongly-typed-id-capabilities.md), which extends strongly typed identifiers with a standardized set of convenience capabilities for common identifier operations.

The project already uses strongly typed IDs to improve type safety and domain expressiveness. This decision builds on that foundation by standardizing creation, empty-value access, parsing, and safe conversion behavior, especially for `Guid`-based identifiers.

This validation is important because identifier handling often crosses domain, application, and infrastructure boundaries. Without a consistent and centralized API for common operations such as `New`, `Empty`, `Parse`, and `TryParse`, the codebase can accumulate duplicated parsing logic, inconsistent failure behavior, and avoidable boilerplate.

The goal of this plan is to ensure that advanced helper capabilities remain type-safe, predictable, developer-friendly, and fully compatible with the existing equality and persistence semantics established in earlier ADRs.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on helper APIs such as New(), Empty, Parse(string), and TryParse(string, out TId) for strongly typed identifiers.

- **Target Project:**
  - `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

- **Integration Tests:**
  - Focus on verifying that parsed and generated identifiers remain persistence-compatible and behave correctly in real EF Core scenarios.

- **arget Project:**
  - `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

- **Module Integration Tests:**
  - Focus on verifying that real module identifiers such as `ProjectId` expose and use the new capabilities consistently.

- **Target Project:**
  - `tests/IntegrationTests/Hector.Modules.Projects.IntegrationTests`

---

## 1. Scope

### Included

- `New()` creation behavior for `Guid`-based strongly typed IDs
- `Empty` strongly typed identifier behavior
- `Parse(string)` behavior for valid and invalid input
- `TryParse(string, out TId)` behavior for valid and invalid input
- Preservation of equality semantics after parsing and generation
- Use of helper APIs in real module identifier types
- EF Core compatibility of identifiers created or parsed through helper APIs

### Excluded

- JSON serialization customization
- [ASP.NET](https://dotnet.microsoft.com/en-us/apps/aspnet) model binding behavior unless explicitly tested elsewhere
- Generic helper extensions introduced by later ADRs
- Assembly scanning and automated registration concerns
- Non-`Guid` parsing strategies unless explicitly supported by the abstraction

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_CreateValidIdentifier_When_NewIsCalled

**Scenario:**

- Calling `New()` on a concrete strongly typed identifier must produce a valid non-default ID.

**Arrange:**

- Select a concrete `Guid`-based identifier type such as `ProjectId`.

**Act:**

- Call `ProjectId.New()`.

**Assert:**

- A non-null identifier instance is returned.
- Its underlying value is not `Guid.Empty`.

---

### TC-02

- #### Should_ReturnEmptyIdentifier_When_EmptyIsAccessed

**Scenario:**

- Accessing `Empty` must return a strongly typed identifier representing the default empty primitive value.

**Arrange:**

- Select a concrete strongly typed identifier type.

**Act:**

- Access the `Empty` member.

**Assert:**

- A valid identifier object is returned.
- Its underlying value equals `Guid.Empty`.

---

### TC-03

- #### Should_ParseIdentifier_When_ParseReceivesValidGuidString

**Scenario:**

- `Parse(string)` must convert a valid external string value into the correct strongly typed identifier.

**Arrange:**

- Prepare a valid Guid string.

**Act:**

- Call `Parse` on the identifier type.

**Assert:**

- A strongly typed identifier is returned.
- Its underlying value matches the parsed `Guid`.

---

### TC-04

- #### Should_Throw_When_ParseReceivesInvalidGuidString

**Scenario:**

- `Parse(string)` must fail predictably when the provided input cannot be parsed.

**Arrange:**

- Prepare an invalid identifier string.

**Act:**

- Call `Parse`.

**Assert:**

- An exception is thrown.
- The failure is clear and consistent with the parsing contract.

---

### TC-05

- #### Should_ReturnTrueAndParsedId_When_TryParseReceivesValidGuidString

**Scenario:**

- `TryParse` must safely parse valid external input without throwing.

**Arrange:**

- Prepare a valid `Guid` string.

**Act:**

- Call `TryParse(string, out TId id)`.

**Assert:**

- The method returns `true`.
- The output identifier is populated.
- The underlying value matches the expected parsed `Guid`.

---

### TC-06

- #### Should_ReturnFalseAndDefaultOutput_When_TryParseReceivesInvalidGuidString

**Scenario:**

- `TryParse` must reject invalid input without throwing and must return a predictable failure result.

**Arrange:**

- Prepare an invalid identifier string.

**Act:**

- Call `TryParse`.

**Assert:**

- The method returns `false`.
- The out parameter contains the expected default or empty result according to the API contract.

---

### TC-07

- #### Should_BeEqual_When_IdentifierIsParsedFromItsOwnStringRepresentation

**Scenario:**

- Parsing an identifier from its own string representation must preserve semantic identity.

**Arrange:**

- Create a strongly typed identifier with a known value.
- Convert its underlying value to string.

**Act:**

- Parse the string back into the same identifier type.

**Assert:**

- The parsed identifier is equal to the original identifier.
- Their hash codes are equal.

---

### TC-08

- #### Should_NotBeEqual_When_ParsedIdentifiersHaveDifferentValues

**Scenario:**

- Parsed identifiers with different values must remain distinct.

**Arrange:**

- Prepare two different valid `Guid` strings.

**Act:**

- Parse both into the same strongly typed identifier type.

**Assert:**

- The resulting identifiers are not equal.

---

### TC-09

- #### Should_RemainPersistenceCompatible_When_ParsedOrGeneratedIdentifierIsSavedAndLoaded

**Scenario:**

- IDs produced via helper APIs must remain fully compatible with EF Core persistence mapping.

**Arrange:**

- Create or parse an identifier.
- Assign it to a persisted entity.

**Act:**

- Save and reload the entity through EF Core.

**Assert:**

- The restored identifier has the same concrete type.
- The restored identifier preserves the original underlying value.

---

### TC-10

- #### Should_ExposeStandardCapabilities_When_ProjectIdImplementsAdvancedStronglyTypedIdPattern

**Scenario:**

- Real module identifiers must expose the standardized helper capabilities consistently.

**Arrange:**

- Select `ProjectId` as a concrete identifier in the Projects module.

**Act:**

- Evaluate available identifier operations such as `New`, `Empty`, `Parse`, and `TryParse`.

**Assert:**

- `ProjectId` supports the standardized helper API surface.
- Behavior is consistent with the shared abstraction.

---

### TC-11

- #### Should_AvoidDuplicatedParsingLogic_When_ApplicationOrInfrastructureConsumesIdentifierInput

**Scenario:**

- External identifier handling should use standardized strongly typed ID helpers instead of duplicating parsing code.

**Arrange:**

- Inspect representative application or infrastructure flows that consume external identifier values.

**Act:**

- Evaluate identifier conversion approach.

**Assert:**

- Parsing behavior relies on the strongly typed ID abstraction or shared helper pattern.
- Ad hoc parsing logic is minimized.

---

## 3. Non-Functional Validation Points

### 3.1 Developer Experience

Verify that common identifier operations are simple, discoverable, and consistent across modules.

Developers should not need to implement repetitive parsing or empty-value patterns in each domain type.

### 3.2 Contract Stability

Verify that the additional helper APIs extend the abstraction without changing the equality, value encapsulation, or persistence behavior established in [ADR-0008](/docs/adr/0008-strongly-typed-ids.md) and [ADR-0009](/docs/adr/0009-strongly-typed-ids-enhancement.md).

### 3.3 Failure Predictability

Verify that invalid parsing scenarios behave in a clear and stable manner.

`Parse` should fail explicitly, while `TryParse` should fail safely without exceptions.

### 3.4 Persistence Safety

Verify that new helper capabilities do not interfere with EF Core materialization or configured value converters.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - `ProjectId.New()`
  - `ProjectId.Empty`
  - `"11111111-1111-1111-1111-111111111111"`
  - `"22222222-2222-2222-2222-222222222222"`
  - "`invalid-guid`"
  - `string.Empty`
  - `null` where API contract permits validation

- **Expected Outputs:**
  - `New()` returns non-default identifier
  - `Empty` wraps `Guid.Empty`
  - `Parse(valid)` returns typed identifier
  - `Parse(invalid)` throws
  - `TryParse(valid)` returns true
  - `TryParse(invalid)` returns false
  - Persisted parsed/generated IDs round-trip correctly

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  Define failing tests for Empty, Parse, TryParse, and compatibility of parsed/generated identifiers with existing equality and persistence rules.
2. **GREEN:** Implement the minimum helper capabilities required in the strongly typed ID abstraction and concrete identifier types.
3. **REFACTOR:** Remove duplicated parsing and empty-value logic from consumers while preserving clarity, type safety, and green tests.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] `New`, `Empty`, `Parse`, and `TryParse` behave consistently.
- [ ] Invalid parsing behavior is predictable and verified.
- [ ] Equality semantics remain preserved after parsing and generation.
- [ ] EF Core persistence compatibility is verified.
- [ ] Real module identifiers expose the standard helper capabilities.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Domain.UnitTests/
 │       ├── StronglyTypedIdTests.cs
 │       ├── StronglyTypedIdArchitectureTests.cs
 │       └── StronglyTypedIdLayerRulesTests.cs
 └── IntegrationTests/
     ├── Hector.BuildingBlocks.Persistence.IntegrationTests/
     |  └── StronglyTypedIdMappingTests.cs
     └── Hector.Modules.Projects.IntegrationTests/
        └── ProjectsStronglyTypedIdMappingTests.cs
```

---

## Summary

This test plan ensures that advanced strongly typed ID capabilities improve usability without weakening the architectural guarantees already established by earlier decisions. By validating standardized creation, parsing, empty-value handling, and persistence compatibility, the architecture keeps identifiers expressive, safe, and consistent across domain, application, and infrastructure boundaries.

---
