# Test Plan: ADR-0011 Eliminate Boilerplate in Strongly Typed IDs Using Self-Referencing Generics

## Status

Superseded

## Context

This test plan validates the architectural decision defined in [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md), which proposed eliminating repetitive boilerplate in strongly typed identifiers by introducing a self-referencing generic base class specialized for Guid identifiers:

```csharp
StronglyTypedId<TSelf> where TSelf : StronglyTypedId<TSelf>
```

This ADR attempted to simplify the inheritance hierarchy by removing the generic primitive type parameter and centralizing common `Guid`-specific operations such as:

- `New()`
- `Empty`
- `Parse(string)`
- `TryParse(string, out TSelf?)`

It also proposed:

- specialization for `Guid`-only identifiers,
- `Guid.CreateVersion7()` for identifier generation,
- explicit factory methods in concrete types to avoid reflection-based construction.

However, this ADR is marked as **Superseded**, meaning its design is no longer the active architectural direction. Therefore, the purpose of this test plan is not to validate current implementation as the preferred solution, but to verify one of the following outcomes:

1. the ADR was never implemented,
2. the ADR was implemented and later replaced,
3. its intended behavior is now covered by a newer and preferred design.

The goal is to confirm that the superseded design is either absent, deprecated, or safely replaced without leaving architectural inconsistency in the codebase.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Validate that the CRTP-style `StronglyTypedId<TSelf>` abstraction is either not present as the active design, or—if remnants exist—not used as the authoritative strongly typed ID mechanism.

- **Target Project:**
  - `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

- **Architecture Tests:**
  - Validate that the currently accepted abstraction for strongly typed IDs does not depend on [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) as the primary architectural contract.

- **Target Project:**
  - `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

- **Integration Tests:**
  - Validate that persistence and module behavior use the active identifier mechanism rather than the superseded CRTP implementation.

- **Target Project:**
  - `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

### Included

- Verification that [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) is not the active architectural pattern
- Detection of any remaining `StronglyTypedId<TSelf>` CRTP-based implementation
- Validation that active module identifiers do not rely on the superseded pattern as - their primary abstraction
- Verification that current strongly typed ID capabilities are provided through the replacement design
- Verification that persistence mapping does not depend on [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md)-specific implementation details
- Validation of replacement behavior where equivalent functionality still exists

### Excluded

- Re-validating [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) as if it were the accepted design
- Forcing implementation of `Guid.CreateVersion7()` if the replacement ADR changed generation strategy
- Requiring CRTP-based factories or explicit `Create(Guid)` methods unless still part of the active design
- Testing hypothetical transitional code no longer present in the repository

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01

- #### Should_NotUseSelfReferencingStronglyTypedIdAsPrimaryAbstraction_When_ADR0011IsSuperseded

**Scenario:**

- Since [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) is superseded, the codebase should not rely on `StronglyTypedId<TSelf>` as the primary architectural contract for identifiers.
**Arrange:**

- Inspect the currently active strongly typed ID base abstractions.

**Act:**

- Evaluate inheritance and identifier usage patterns across domain types.

**Assert:**

- The active abstraction does not depend primarily on the superseded CRTP model.
- A replacement abstraction exists or the previous abstraction remains authoritative.

---

### TC-02

- #### Should_ConfirmReplacementDesignProvidesEquivalentIdentifierCapabilities

**Scenario:**

- Even though [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) is superseded, the useful behaviors it targeted may still exist through another design.

**Arrange:**

- Identify the active strongly typed ID implementation.

**Act:**

- Evaluate support for common capabilities such as creation, parsing, empty values, and type safety.

**Assert:**

- Equivalent or improved capabilities are available in the replacement approach.
- There is no functional regression caused by superseding [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md).

---

### TC-03

- #### Should_NotRequireConcreteIdentifiersToFollowCRTPPattern_When_ActiveDesignDiffers

**Scenario:**

- Concrete identifier types should not be forced into the `public sealed class XId : StronglyTypedId<XId>` pattern if that design is no longer active.

**Arrange:**

- Inspect representative identifier types such as `ProjectId`.

**Act:**

- Evaluate their inheritance structure.

**Assert:**

- Identifier types conform to the active architecture rather than the superseded CRTP pattern.
- No architecture test incorrectly enforces [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) rules.

---

### TC-04

- #### Should_NotDependOnExplicitCreateGuidFactoryForBaseInstantiation_When_ADR0011Was

**Scenario:**

- If [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) is superseded, the system should not require the exact explicit factory contract proposed by that ADR unless retained intentionally.

**Arrange:**

- Inspect active identifier creation mechanisms.

**Act:**

- Evaluate whether concrete identifiers must expose `Create(Guid)` specifically to support the base class.

**Assert:**

- Current construction patterns follow the active design.
- No obsolete dependency on [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md)-specific factory conventions remains unintentionally.

---

### TC-05

- #### Should_VerifyCurrentIdentifierGenerationStrategyMatchesActiveADR_NotSupersededOne

**Scenario:**

- [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) proposed `Guid.CreateVersion7()` specifically. Since it is superseded, generation strategy must be validated against the currently accepted ADR instead.

**Arrange:**

- Inspect current identifier generation logic.

**Act:**

- Compare implementation behavior with the active architectural decision.

**Assert:**

- Generation strategy aligns with the accepted ADR.
- The system does not assume [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) generation rules unless intentionally preserved.

---

### TC-06

- #### Should_RemainPersistenceCompatible_WithoutRequiringCRTPSpecificBaseType

**Scenario:**

- Persistence compatibility must be preserved regardless of whether the superseded CRTP abstraction exists.

**Arrange:**

- Create and persist entities using active strongly typed identifiers.

**Act:**

- Save and reload entities through EF Core.

**Assert:**

- Identifiers round-trip correctly.
- Persistence does not depend on [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md)-specific inheritance or factory details.

---

### TC-07

- #### Should_AllowModuleIdentifiersToUseActiveAbstractionWithoutBoilerplateRegression

**Scenario:**

- Superseding [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) must not reintroduce unnecessary repetitive boilerplate in module identifiers unless explicitly accepted by the replacement design.

**Arrange:**

- Inspect representative module identifier types.

**Act:**

- Compare their implementation shape against the active abstraction.

**Assert:**

- Identifier implementations remain reasonably concise.
- Current design does not cause avoidable duplication beyond what the accepted architecture requires.

---

### TC-08

- #### Should_DocumentSupersededStatusClearlyThroughArchitectureTestsOrDocumentationValidation

**Scenario:**

- Since [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) is superseded, the codebase or related documentation should make it clear that this is not the authoritative direction.

**Arrange:**

- Inspect ADR references, architecture tests, and related documentation conventions.

**Act:**

- Evaluate whether active tests and rules align with current ADR status.

**Assert:**

- No automated rule treats [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) as mandatory.
- Documentation and architecture expectations align with its superseded state.

---

## 3. Non-Functional Validation Points

### 3.1 Architectural Clarity

Verify that developers can clearly identify which strongly typed ID design is active and which one is superseded.

There must be no ambiguity in architecture tests or base abstractions.

### 3.2 Regression Safety

Verify that superseding [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) did not remove useful capabilities such as centralized creation or parsing unless intentionally replaced with a better alternative.

### 3.3 Boilerplate Control

Verify that the replacement design still keeps repetitive identifier code under control, even if the exact CRTP pattern was abandoned.

### 3.4 Persistence Stability

Verify that replacing or discarding ADR-0011 has no negative impact on EF Core mappings, value conversions, or aggregate persistence behavior.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Current active identifier type such as `ProjectId`
  - Current strongly typed ID base abstraction
  - Identifier creation APIs in accepted implementation
  - Persisted entity using active identifier mapping
  - Architecture inspection of inheritance pattern

- **Expected Outputs:**
  - [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) is not enforced as active architecture
  - Equivalent identifier capabilities exist in the accepted design
  - Persistence works through the active abstraction
  - Module identifiers follow current standards, not superseded ones

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Write tests that would fail if the codebase still incorrectly enforces ADR-0011 as the active identifier architecture.
2. **GREEN:**  Align identifier abstractions, architecture rules, and persistence behavior with the actually accepted ADR.
3. **REFACTOR:** Remove obsolete CRTP assumptions, outdated architecture assertions, or deprecated helper conventions that no longer reflect the active design.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] Architecture tests confirm [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) is not enforced as the active design.
- [ ] Current strongly typed ID implementation provides equivalent required capabilities.
- [ ] Module identifiers follow the active abstraction consistently.
- [ ] Persistence compatibility is verified using the accepted design.
- [ ] No obsolete [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md)-specific rules remain unintentionally enforced.
- [ ] Documentation reflects the superseded status correctly.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Domain.UnitTests/
 │       ├── StronglyTypedIdTests.cs
 │       ├── StronglyTypedIdArchitectureTests.cs
 │       └── StronglyTypedIdSupersededAdrTests.cs
 ├── IntegrationTests/
 │   └── Hector.BuildingBlocks.Persistence.IntegrationTests/
 │       └── StronglyTypedIdMappingTests.cs
 └── IntegrationTests/
     └── Hector.Modules.Projects.IntegrationTests/
         └── ProjectsStronglyTypedIdMappingTests.cs
```

---

## Summary

This test plan validates [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) in the correct architectural context: not as an accepted implementation target, but as a **superseded proposal** whose intent must either be absent, replaced, or preserved through a better design. The focus is on ensuring architectural clarity, avoiding obsolete enforcement, and confirming that current strongly typed ID behavior remains consistent, safe, and persistence-compatible.

---
