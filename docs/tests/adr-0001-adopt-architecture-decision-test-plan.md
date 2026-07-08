# Test Plan: ADR-0001 Adopt Architecture Decision Records (ADR)

## Status

Accepted

## Context

This test plan validates the implementation of [ADR-0001](/docs/adr/0001-adopt-architecture-decision-records.md). Its purpose is to ensure that architectural decisions are recorded in a structured, sequential, and consistent manner. These tests act as Architecture Guards to prevent documentation decay and maintain the integrity of the decision log.

## Test Strategy

Since this ADR is process-oriented, the validation relies on Architecture Guard Tests that inspect the file system and Markdown structure. The test is implemented using a custom fluent DSL (`ArchitectureRule`) to provide a single, readable, and comprehensive check.

- **Architecture Tests**: A single, automated check for directory existence, file naming conventions, mandatory Markdown headers, and numbering integrity. (Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`)

---

## 1. Scope

### Included

- The `docs/adr` directory structure.
- File naming conventions: `NNNN-kebab-case-title.md`.
- Presence of mandatory sections: `Status`, `Context`, `Decision`, `Consequences`.
- Uniqueness and sequential integrity of ADR numbers, with a recognized exception for number `42`.

### Excluded

- Qualitative analysis of the text/grammar within ADRs.
- Other documentation files outside the `docs/adr` path.

## 2. Test Case (Architecture Guard Test)

### TC-01..03: Should_AdhereToDefinedStructure_When_EvaluatingAdrFiles

**Scenario:** Verify that all ADR files in the `docs/adr` directory collectively adhere to the project's structural and organizational standards. This single test case covers naming, content, and numbering rules.

**Arrange:**

- The test uses the `ArchitectureRule` DSL to target documentation files of type ADR located in the `docs/adr` directory.

**Act:**

- A chain of rule methods (`.FollowNamingConvention()`, `.ContainMandatorySections(...)`, `.HaveUniqueAndSequentialNumbers(...)`) defines the required structure.
- The `.Check()` method is invoked, which triggers the evaluation of all specified rules against the target files.

**Assert:**

- The test passes only if all three of the following conditions are met:
  1. **Naming Convention:** All files must comply with the `'{number}-{kebab-case-title}.md'` pattern.
  2. **Mandatory Sections:** All files must contain these four sections: `## Status`, `## Context`, `## Decision`, and `## Consequences`.
  3. **Numbering Integrity:** All ADR numbers must be unique and sequential, except for the intentionally reserved number `42`.

---

## 3. Non-Functional Validation Points

### 3.1 Traceability

Ensure ADR files are committed to Git alongside the code changes they describe (verified during Code Review).

---

## 4. Test Data

- **Inputs:** Current files in `docs/adr/*.md`.
- **Expected Outputs:** The `Should_AdhereToDefinedStructure_When_EvaluatingAdrFiles` test passes successfully.

---

## 5. TDD Execution Plan

1. **RED**: Create a test in `Hector.ArchitectureTests` using the DSL that fails if the ADR directory is empty or if a file violates any of the structural rules.
2. **GREEN**: Initialize the `docs/adr` directory and add ADR-0001 with the correct format, ensuring all rules in the chain pass.
3. **REFACTOR**: Refine the rule implementation (e.g., the Markdown parser) to be more resilient to whitespace variations or other minor inconsistencies.

---

## 6. Exit Criteria

- [x] The architecture guard test for ADR structure (`Should_AdhereToDefinedStructure_When_EvaluatingAdrFiles`) passes.
- [x] Existing ADRs (0001 to 0057) are compliant with the structure.

---

## 7. Proposed Test File Layout

```text
tests/
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         └── Documentation/
             └── AdrStructureTests.cs
```

## Summary

This test plan ensures that Hector’s “Architectural Memory” remains organized and machine-verifiable, preventing the documentation from becoming a “Black Box” over time.
