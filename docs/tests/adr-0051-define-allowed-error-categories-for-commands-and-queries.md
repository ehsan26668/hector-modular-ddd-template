# Test Plan: ADR-0051 Define Allowed Error Categories for Commands and Queries

## Status

Accepted

## Context

This test plan validates the error category usage rules defined in [ADR-0051](/docs/adr/0051-define-allowed-error-categories-for-commands-and-queries.md).

ADR‑0050 introduced a standardized error taxonomy for the Application Layer.

However, not all error categories are appropriate for every operation type.

Commands and queries have different responsibilities in a CQRS architecture:

Commands:

- modify system state
- enforce domain invariants
- may encounter concurrency conflicts
- may produce business rule violations

Queries:

- read data from read models
- must not enforce domain business rules
- must not produce write-related conflicts

ADR‑0051 therefore defines which error categories are allowed for each operation type.

Allowed categories must be enforced through architecture rules and automated tests to maintain strict CQRS separation.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests**
  - Validate error category creation and assignment.
  - Validate command and query failure behavior.
  - Target Project: `tests/UnitTests/Hector.Application.UnitTests`

- **Integration Tests**
  - Validate failure behavior through the application execution pipeline.
  - Validate correct failure categories during command/query execution.
  - Target Project: `tests/IntegrationTests/Hector.Application.IntegrationTests`

- **Architecture Tests**
  - Enforce allowed error categories for commands and queries.
  - Prevent invalid category usage.
  - Target Project: `tests/ArchitectureTests`

---

## 1. Scope

- **Included:**
  - Command failure category enforcement
  - Query failure category enforcement
  - CQRS error semantics separation
  - Architecture rule validation
  - Failure behavior consistency

- **Excluded:**
  - Error taxonomy definition (ADR‑0050)
  - Validation pipeline behavior (ADR‑0048)
  - Query Result contract (ADR‑0049)

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_AllowAllDefinedFailureCategories_ForCommands

**Scenario:**  
Commands must support all defined failure categories allowed by ADR‑0051.

Allowed categories:

- Validation
- Unauthorized
- Forbidden
- NotFound
- Conflict
- BusinessRule
- Infrastructure
- Unexpected

**Arrange:**

Create command execution scenarios producing each allowed category.

**Act:**

Execute commands through the application pipeline.

**Assert:**

Verify each failure category is accepted and returned as Result.Failure.

---

### TC-02: Should_AllowOnlyPermittedFailureCategories_ForQueries

**Scenario:**  
Queries must only produce allowed failure categories.

Allowed categories:

- Validation
- Unauthorized
- Forbidden
- NotFound
- Infrastructure
- Unexpected

**Arrange:**

Execute queries producing allowed failure scenarios.

**Act:**

Execute queries through the application pipeline.

**Assert:**

Verify Result failures use only permitted categories.

---

### TC-03: Should_PreventBusinessRuleFailures_FromQueries

**Scenario:**  
Query handlers must not produce BusinessRule errors.

**Arrange:**

Analyze query handler implementations.

**Act:**

Run architecture validation.

**Assert:**

Verify query handlers do not reference errors categorized as BusinessRule.

---

### TC-04: Should_PreventConflictFailures_FromQueries

**Scenario:**  
Query handlers must not produce Conflict errors.

**Arrange:**

Analyze query handler implementations.

**Act:**

Run architecture validation.

**Assert:**

Verify query handlers do not reference errors categorized as Conflict.

---

### TC-05: Should_AllowBusinessRuleFailures_ForCommands

**Scenario:**  
Commands must support domain invariant violations.

**Arrange:**

Create a command that violates a domain rule.

**Act:**

Execute the command.

**Assert:**

Verify response is Result.Failure.

Verify error category equals BusinessRule.

---

### TC-06: Should_AllowConflictFailures_ForCommands

**Scenario:**  
Commands may encounter concurrency conflicts.

**Arrange:**

Simulate DbUpdateConcurrencyException during command execution.

**Act:**

Execute the command.

**Assert:**

Verify Result.Failure returned.

Verify category equals Conflict.

---

### TC-07: Should_AllowInfrastructureFailures_ForCommandsAndQueries

**Scenario:**  
Infrastructure failures may occur in both commands and queries.

**Arrange:**

Simulate database or external service failure.

**Act:**

Execute both command and query operations.

**Assert:**

Verify Result.Failure returned.

Verify category equals Infrastructure.

---

### TC-08: Should_AllowUnexpectedFailures_ForCommandsAndQueries

**Scenario:**  
Unexpected exceptions may occur during execution.

**Arrange:**

Throw runtime exception in execution pipeline.

**Act:**

Execute command and query.

**Assert:**

Verify exception does not propagate.

Verify Result.Failure returned.

Verify category equals Unexpected.

---

### TC-09: Should_EnsureQueriesDoNotExecuteDomainRuleLogic

**Scenario:**  
Queries must not execute domain rule evaluation.

**Arrange:**

Analyze query handlers and dependencies.

**Act:**

Run architecture validation.

**Assert:**

Verify queries do not reference:

- Domain rule validation services
- Domain invariant enforcement methods

This ensures BusinessRule failures cannot occur in query processing.

---

### TC-10: Should_EnsureCommandsMayInvokeDomainRuleValidation

**Scenario:**  
Commands may execute domain rule validation logic.

**Arrange:**

Create command invoking domain invariant logic.

**Act:**

Execute command.

**Assert:**

Verify BusinessRule failure can be produced.

---

## 3. Non-Functional Validation Points

### 3.1 CQRS Integrity

- Verify strict separation between command and query semantics.
- Verify query processing does not execute domain rules.

### 3.2 Consistency

- Verify error categories remain consistent with taxonomy rules.

### 3.3 Observability

- Verify failure categories allow accurate monitoring and alerting.

### 3.4 Architectural Integrity

- Verify architecture tests enforce command/query error policies.

---

## 4. Test Data

- **Inputs:**
  - Command violating domain rule
  - Command with concurrency conflict
  - Query for missing resource
  - Unauthorized query access
  - Infrastructure failure simulation
  - Unexpected runtime exception

- **Expected Outputs:**
  - Result.Failure(Validation)
  - Result.Failure(NotFound)
  - Result.Failure(Conflict)
  - Result.Failure(BusinessRule)
  - Result.Failure(Infrastructure)
  - Result.Failure(Unexpected)

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. RED

Write failing tests for:

- query category restrictions
- command category allowances
- architecture enforcement rules

1. GREEN

Implement:

- command/query category validation
- architecture enforcement tests
- query restriction checks

1. REFACTOR

Improve enforcement utilities and reduce duplication across architecture tests.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Architecture tests enforce category restrictions.
- [ ] Queries cannot produce BusinessRule failures.
- [ ] Queries cannot produce Conflict failures.
- [ ] Commands support all allowed categories.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Application.UnitTests/
 │       ├── CommandFailureCategoryTests.cs
 │       └── QueryFailureCategoryTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.Application.IntegrationTests/
 │       └── CommandQueryFailureBehaviorTests.cs
 │
 └── ArchitectureTests/
     ├── QueryErrorCategoryRestrictionTests.cs
     └── CommandErrorCategoryAllowanceTests.cs
```

## Summary

This test plan validates the error category usage rules defined in ADR‑0051.

It ensures that:

- commands and queries follow different failure semantics
- query handlers cannot produce
