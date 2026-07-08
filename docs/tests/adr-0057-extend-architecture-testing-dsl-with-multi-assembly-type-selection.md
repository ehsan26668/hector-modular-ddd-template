# Test Plan: ADR-0057 Extend Architecture Testing DSL With Multi-Assembly Type Selection

## Status

Accepted

## Context

This test plan validates the **Multi-Assembly Type Selection** feature of the Hector Architecture Testing DSL described in [ADR-0057](/docs/adr/0057-extend-architecture-testing-dsl-with-multi-assembly-type-selection.md).

In a Modular DDD system, governance rules are not isolated to single projects; they are applied to logical layers (e.g., Domain layer spanning multiple feature domain assemblies) or modules. Testing the DSL's capability to process multiple assemblies correctly and aggregate types without duplicates or omission is critical.

This test plan ensures the DSL aggregates assemblies correctly, integrates seamlessly with the existing `ArchitectureRule` pipeline, and provides exact diagnostics without leakage or failure.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Verify that the `TypesSelection` class correctly processes multiple assemblies and aggregates them.
  - Verify that the fluent DSL filters types across multiple assemblies correctly.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests/Framework/Unit/RuleTests`

- **Integration / System Tests:**
  - Self-testing architectural violations using multi-assembly inputs to verify that `EvaluationResult` captures violations across boundaries.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests/LayerBoundaries`

---

## 1. Scope

- **Included:**
  - Validation of `ITypesSelection.That(IEnumerable<Assembly>)` behavior.
  - Verification of type collection aggregation using `NetArchTest.Rules.Types.InAssemblies`.
  - Diagnostics and failure formatting for multi-assembly violations.
  - Migration validation for Layer Isolation Rules (`LayerDependencyTests`).

- **Excluded:**
  - Validation of single-assembly selection APIs (already covered by ADR-0056).
  - Internal NetArchTest evaluation mechanics.

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_AggregateTypesFromAllAssemblies_When_MultiAssemblySelectionIsUsed

**Scenario:** The DSL is invoked with multiple assemblies containing distinct types, and we check if the selection gathers types from all target assemblies.

**Arrange:**

- Select two distinct assemblies from the project (e.g., `Hector.BuildingBlocks.Domain` and `Hector.BuildingBlocks.Application`).
- Setup the DSL selection using `ArchitectureRule.Types().That(new[] { domainAssembly, applicationAssembly })`.

**Act:**

- Retrieve or filter types (e.g., matching a wildcard namespace namespace prefix `Hector.BuildingBlocks.*`).

**Assert:**

- Verify that types from both assemblies are present in the final list.
- Verify that no type from other unselected assemblies is included.

### TC-02: Should_ThrowArgumentNullException_When_AssembliesCollectionIsNull

**Scenario:** Passing a null collection of assemblies to the `That` API should immediately fail with a clean argument exception.

**Arrange:**

- Prepare a null `IEnumerable<Assembly>` variable.

**Act:**

- Invoke `ArchitectureRule.Types().That(nullAssemblies)`.

**Assert:**

- Verify that an `ArgumentNullException` is thrown.

### TC-03: Should_ReportViolationsAcrossAllAssemblies_When_RuleFailsOnMultipleAssemblies

**Scenario:** A rule is evaluated against multiple assemblies, and some of them contain violations. The output report must accurately list violations from all contributing assemblies.

**Arrange:**

- Select assemblies containing known test violations (or mock types using self-testing constructs).
- Define a rule specifying that classes should not depend on a specific namespace.

**Act:**

- Evaluate the rule using `.Evaluate()`.

**Assert:**

- Verify that the rule fails.
- Assert that the exception message or diagnostic report lists violating types originating from multiple different assemblies.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that evaluation error messages do not leak system path configuration, local environment paths, or internal tooling structures. Only type names and dependency references should be exposed in the diagnostic details.

### 3.2 Observability & Traceability

- Ensure that the evaluation diagnostics output format remains consistent with ADR-0056, listing rule descriptions, reasons (`Because`), and violation counts.

### 3.3 Contract Stability

- Verify that the `ITypesSelection` signature updates do not break compilation for existing single-assembly `.That(assembly)` or parameterless `.That()` calls.

---

## 4. Test Data

- **Inputs:**
  - Collection of assemblies: `[ Hector.BuildingBlocks.Domain, Hector.BuildingBlocks.Application ]`
  - Null reference input
  - Single item array input: `[ Hector.BuildingBlocks.Domain ]`

- **Expected Outputs:**
  - Aggregated list of all types residing within the inputs.
  - Standardized exception format: `ArgumentNullException`
  - Violation diagnostics string listing violating types and target assemblies.

---

## 5. TDD Execution Plan

1. **RED**
   - Write a unit test verifying multi-assembly type aggregation in `TypesSelectionTests` using the new `That(IEnumerable<Assembly>)` signature (fails to compile/run).
   - Write a boundary test for the null check constraint.

2. **GREEN**
   - Implement the `IEnumerable<Assembly>` overload on `ITypesSelection` and `TypesSelection`.
   - Ensure the internal wrapper calls `Types.InAssemblies(assemblies)`.
   - Run tests until they pass.

3. **REFACTOR**
   - Clean up code formatting.
   - Refactor `LayerDependencyTests` and `ModuleIsolationTests` to use the new multi-assembly DSL APIs.
   - Verify that all architectural guard tests compile and pass.

---

## 6. Exit Criteria

- [ ] `ITypesSelection.That(IEnumerable<Assembly>)` is fully implemented.
- [ ] All Unit Tests for the DSL selection pass.
- [ ] Layer Dependency and Module Isolation tests migrated to the new API and passing.
- [ ] `decision-log.md` updated with ADR-0057 status.

---

## 7. Proposed Test File Layout

```text
tests/
    └── ArchitectureTests/
        └── Hector.ArchitectureTests/
            ├── Framework/
            │   └── Unit/
            │       └── RuleTests/
            │           └── RuleBuilderTests.cs (extended for multi-assembly cases)
            └── LayerBoundaries/
                ├── LayerDependencyTests.cs (migrated)
                └── ModuleIsolationTests.cs (migrated)
```

## Summary

This test plan ensures that ADR-0057 is validated against the expected architectural and runtime behavior.
The result should improve system quality, reliability, and maintainability while preserving the modular boundaries defined by the architecture.
