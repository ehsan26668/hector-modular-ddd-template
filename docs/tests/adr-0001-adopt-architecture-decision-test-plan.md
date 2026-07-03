# Test Plan: ADR-0001 Adopt Architecture Decision Records (ADR)

## Status

Accepted

## Context

This test plan validates the implementation of [ADR-0001](/docs/adr/0001-adopt-architecture-decision-records.md). Its purpose is to ensure that architectural decisions are recorded in a structured, sequential, and consistent manner. These tests act as Architecture Guards to prevent documentation decay and maintain the integrity of the decision log.

## Test Strategy

Since this ADR is process-oriented, the validation relies on Architecture Guard Tests that inspect the file system and Markdown structure.

- **Architecture Tests**: Automated checks for directory existence, file naming conventions, and mandatory Markdown headers. (Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`)

---

## 1. Scope

### Included

- The `docs/adr` directory structure.
- File naming conventions (4-digit prefix).
- Presence of mandatory sections: `Status`, `Context`, `Decision`, `Consequences`.
- Uniqueness and sequential integrity of ADR numbers.

### Excluded

- Qualitative analysis of the text/grammar within ADRs.
- Other documentation files outside the `docs/adr` path.

## 2. Test Cases (Architecture Guard Tests)

### TC-01: Should_FollowNamingConvention_When_ADRFileIsCreated

**Scenario:**  Verify that every ADR file starts with a 4-digit index followed by a slug.

**Arrange:**

- Scan the `docs/adr` directory.

**Act:**

- Validate file names against the regex `^\d{4}-.*\.md$`.

**Assert:**

- All files must comply with the naming pattern.

### TC-02: Should_ContainMandatorySections_When_ADRFileExists

**Scenario:**  Ensure each ADR maintains the required structural headers.

**Arrange:**

- Load content of each `.md` file in the ADR folder.

**Act:**

- Search for headers: `## Status`, `## Context`, `## Decision`, and `## Consequences`.

**Assert:**

- All files must contain these four sections.

### TC-03: Should_HaveUniqueAndSequentialNumbers_When_ScanningADRDirectory

**Scenario:**  Prevent duplicate ADR numbers and ensure a logical timeline.

**Arrange:**

- Extract the numeric prefix from all ADR filenames.

**Act:**

- Check for duplicates and gaps in the sequence.

**Assert:**

- No duplicate IDs; sequence should be continuous (allowing for planned gaps if documented).

---

## 3. Non-Functional Validation Points

### 3.1 Traceability

Ensure ADR files are committed to Git alongside the code changes they describe (verified during Code Review).

---

## 4. Test Data

- **Inputs:** Current files in `docs/adr/*.md`.
- **Expected Outputs:** All Architecture Guard tests in the test suite pass.

---

## 5. TDD Execution Plan

1. **RED**: Create a test in `Hector.ArchitectureTests` that fails if the ADR directory is empty or if a file violates the naming convention.
2. **GREEN**: Initialize the `docs/adr` directory and add ADR-0001 with the correct format.
3. **REFACTOR**: Refine the Markdown parser logic to be more resilient to whitespace variations in headers.

---

## 6. Exit Criteria

- [x] Architecture Guard tests for ADR structure pass.
- [x]  Existing ADRs (0001 to 0055) are compliant with the structure.

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
