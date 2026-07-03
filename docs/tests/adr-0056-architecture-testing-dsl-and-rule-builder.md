# Test Plan: ADR-0056 Introduce Architecture Testing DSL and Rule Builder

## Status

Accepted

## Context

This test plan validates the **Architecture Testing DSL and Rule Builder** described in [ADR-0056](/docs/adr/0056-architecture-testing-dsl-and-rule-builder.md).  
This ADR is critical because it introduces a unified, fluent, and deterministic mechanism for declaring and evaluating architecture rules inside Hector. It addresses an important architectural governance concern: the existing architecture guard tests correctly validate boundaries, but they rely too heavily on low-level NetArchTest APIs, repetitive assertion patterns, and inconsistent diagnostics.

This behavior matters because Hector is built as a modular DDD system where architectural drift can silently break:

- layer isolation,
- module boundaries,
- CQRS conventions,
- result-pattern policies,
- dependency restrictions,
- and domain purity.

The Architecture Testing DSL must therefore be validated as an explicit governance mechanism that improves readability, standardizes assertions, centralizes diagnostics, enables reusable convention packs, and preserves deterministic architecture validation behavior.  
This test plan verifies both the functional correctness of the DSL and the non-functional guarantees required for CI/CD-safe architecture enforcement.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on fluent DSL composition, rule metadata, evaluation behavior, violation aggregation, convention pack execution, diagnostic normalization, and deterministic output.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`

- **Integration Tests:**
  - Focus on end-to-end execution of architecture rules against real assemblies, real namespace filtering, dependency detection, and wrapper integration with NetArchTest.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

- **Included:**
  - Fluent DSL rule definition using `ArchitectureRule.Types()`, `That(...)`, `ResideInNamespace(...)`, `Should()`, `NotDependOn(...)`, `Build(...)`, and `Because(...)`
  - Rule builder evaluation pipeline and `EvaluationResult` behavior
  - Rule metadata validation including `Id`, `Name`, and `Reason`
  - Dependency violation detection using real assemblies
  - Aggregation of multiple rule results through `ArchitectureRuleSet`
  - Standardized violation contracts using `ArchitectureViolation`
  - Standardized report generation through `ArchitectureEvaluationReport`
  - Deterministic ordering of aggregated violations
  - Diagnostic sanitization and normalization
  - Convention pack execution through `Conventions.*`
  - Traceability of diagnostics to exact rule identifiers and reasons
  - Self-testing violation scenarios to verify rule enforcement behavior

- **Excluded:**
  - Roslyn analyzers
  - Source generators
  - IDE diagnostics
  - Compile-time architecture enforcement
  - Advanced dependency graph traversal planned for later phases

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_CreateAndEvaluateArchitectureRule_When_UsingFluentDsl

**Scenario:** A readable architecture rule is created through the fluent DSL and evaluated through the architecture testing framework.

**Arrange:**

- Create a rule using the fluent entry point.
- Define rule metadata including identifier, name, and reason.
- Use a test assembly or evaluation delegate to validate the rule pipeline.
- Prepare an assertion or evaluation path that can be executed deterministically.

**Act:**

- Build the rule.
- Execute `Evaluate()` or `EvaluateWithResult()`.

**Assert:**

- Verify the rule is created successfully.
- Verify `Id`, `Name`, and `Reason` are assigned correctly.
- Verify the underlying assertion or evaluator is executed.
- Verify the result shape is consistent and deterministic.
- Verify fluent syntax remains readable and composable.

### TC-02: Should_ReportViolations_When_DependencyBoundaryIsBroken

**Scenario:** A forbidden dependency exists inside a target namespace and must be detected through the DSL-backed rule builder.

**Arrange:**

- Provide a real assembly containing an intentional dependency on a forbidden package or namespace.
- Define a rule using:
  - `ArchitectureRule.Types()`
  - `That(targetAssembly)`
  - `ResideInNamespace(...)`
  - `Should()`
  - `NotDependOn(...)`
  - `Build(...)`
  - `Because(...)`
- Ensure the selected namespace includes at least one violating type.

**Act:**

- Evaluate the rule through `EvaluateWithResult()`.

**Assert:**

- Verify `HasViolations` is `true`.
- Verify diagnostics are returned.
- Verify diagnostics identify the offending type(s).
- Verify diagnostics include the violated dependency policy in a readable form.
- Verify output follows the standardized diagnostic format.
- Verify violation detection works against real assembly scanning and not only synthetic predicates.

### TC-03: Should_ExecuteConventionPackAndAggregateViolations_When_UsingPredefinedPolicies

**Scenario:** A predefined convention pack or grouped rule execution is evaluated and returns a deterministic aggregated report.

**Arrange:**

- Create multiple architecture rules or use predefined conventions such as:
  - `Conventions.LayerIsolation(...)`
  - `Conventions.DomainPurity(...)`
  - `Conventions.CQRS(...)`
  - `Conventions.ResultPattern(...)`
- Register them in an `ArchitectureRuleSet`.
- Prepare assemblies and rules that produce one or more violations.

**Act:**

- Execute the rule set evaluation.
- Generate an `ArchitectureEvaluationReport`.

**Assert:**

- Verify all registered rules are executed.
- Verify all violations are aggregated into a single report.
- Verify each violation includes:
  - `RuleId`
  - `RuleName`
  - `Reason`
  - `Diagnostic`
- Verify the aggregated output order is deterministic.
- Verify the report can produce normalized diagnostic text.
- Verify reusable convention packs behave as a stable policy abstraction.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that no sensitive information, stack traces, reflection internals, or internal framework implementation details leak through diagnostics.
- Verify that raw `System.Reflection` details, stack-trace fragments, or similar internal diagnostic noise are sanitized or redacted.
- Verify that generated diagnostic output is safe for CI logs and external review.
- Verify that newline-heavy or noisy diagnostic payloads are normalized into stable output.

### 3.2 Observability & Traceability

- Verify that every violation can be traced back to the exact architectural rule through `RuleId` and `RuleName`.
- Verify that rule intent is preserved through the `Reason` field.
- Verify that aggregated reports remain inspectable and suitable for CI diagnostics.
- Verify that rule execution results can be correlated to architectural policies rather than low-level NetArchTest mechanics.

### 3.3 Contract Stability

- Verify that the fluent DSL entry points and chaining model remain stable and predictable.
- Verify that convention pack names and rule composition patterns do not change unintentionally.
- Verify that `EvaluationResult`, `ArchitectureViolation`, and `ArchitectureEvaluationReport` preserve stable shape and semantics.
- Verify that diagnostic output remains deterministic across repeated executions and framework upgrades.

---

## 4. Test Data

- **Inputs:**
  - Valid assemblies without forbidden dependencies
  - Assemblies with intentional dependency violations
  - Namespace selections targeting framework test fixtures
  - Rules with explicit identifiers, names, and reasons
  - Multiple rules aggregated into a single `ArchitectureRuleSet`
  - Diagnostic messages containing reflection terms or stack-trace-like fragments for sanitization checks

- **Expected Outputs:**
  - Stable and deterministic rule evaluations
  - `EvaluationResult.Success()` for compliant rules
  - `EvaluationResult.Failure(...)` for violating rules
  - Structured `ArchitectureViolation` entries for each failure
  - Deterministically ordered `ArchitectureEvaluationReport`
  - Sanitized and standardized diagnostic messages
  - Convention pack execution results with aggregated diagnostics

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED**
   - Define failing tests for fluent syntax entry points and rule metadata behavior.
   - Define failing tests for dependency violation detection through the DSL.
   - Define failing tests for aggregation, deterministic ordering, and diagnostic traceability.
   - Define failing tests for sanitization and non-leakage of internal framework details.

2. **GREEN**
   - Implement the minimal fluent DSL chain required to create and evaluate architecture rules.
   - Implement `ArchitectureRule`, `EvaluationResult`, and the rule builder pipeline.
   - Implement dependency checks by wrapping NetArchTest behavior.
   - Implement `ArchitectureRuleSet`, `ArchitectureViolation`, and `ArchitectureEvaluationReport`.
   - Implement predefined convention pack support.
   - Implement diagnostic sanitization and standardized report formatting.

3. **REFACTOR**
   - Simplify the DSL composition model while preserving readability.
   - Extract reusable abstractions for rule creation and grouped execution.
   - Normalize diagnostics and deterministic ordering behavior.
   - Strengthen naming, metadata consistency, and reusable convention pack boundaries.
   - Reduce direct exposure of low-level NetArchTest mechanics.

---

## 6. Exit Criteria

- [x] All Architecture DSL Unit-level tests pass.
- [x] All architecture evaluation (assembly-scanning) tests pass.
- [x] Security and non-functional points are verified.
- [x] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         └── Framework/
             └── Unit/
                 ├── FluentSyntaxTests.cs
                 ├── RuleBuilderTests.cs
                 ├── ArchitectureRuleTests.cs
                 ├── ConventionPackTests.cs
                 └── ArchitectureDiagnosticsTests.cs
```

## Summary

This test plan ensures that [ADR-0056](/docs/adr/0056-architecture-testing-dsl-and-rule-builder.md) is validated against the expected architectural and runtime behavior of Hector’s architecture governance framework.  
The result improves readability, consistency, traceability, and maintainability of architecture guard tests while preserving deterministic enforcement of modular DDD boundaries.  
It also establishes the testing foundation required for future evolution toward more advanced rule composition, dependency graph analysis, and Roslyn-based architecture enforcement.
