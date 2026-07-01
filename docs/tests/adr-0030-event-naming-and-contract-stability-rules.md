# Test Plan: ADR-0030 Event Naming and Contract Stability Rules

## Status

Accepted

## Context

This test plan validates the **Event Naming and Contract Stability Rules** defined in [ADR-0030](/docs/adr/0030-event-naming-and-contract-stability-rules.md).
As the system evolves, consistent naming conventions and contract isolation are critical to prevent coupling and ensure backward compatibility. This plan enforces the structural and naming rules required for maintainable event-driven communication across module boundaries.

## Test Strategy

- **Architectural Tests:**
  - Focus on enforcing naming conventions (suffixes, past tense), correct project location (Contracts projects), and prohibited dependencies (e.g., Domain layer referencing Integration events).
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`

- **Unit Tests:**
  - Focus on contract purity (record types, no logic) and structural validation.
  - Target Project: `tests/UnitTests/[ProjectName].UnitTests`

---

## 1. Scope

- **Included:**
  - Enforcement of `*DomainEvent` and `*IntegrationEvent` naming suffixes.
  - Verification that integration events reside in the designated `Contracts` namespace/project.
  - Verification that integration events are immutable (`sealed record`).
  - Validation of event contract purity (no methods, no logic).
  - Prohibition of domain layer references to `IIntegrationEvent`.

- **Excluded:**
  - Runtime performance of event dispatching.
  - Consumer-side logic execution.

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_FollowNamingConvention_ForDomainAndIntegrationEvents

**Scenario:** All event classes must end with the correct suffix and use past-tense verbs to describe a fact.

**Arrange:**

- Use `NetArchTest.Rules` to scan event assemblies.

**Act:**

- Apply rules checking for `*DomainEvent` and `*IntegrationEvent` suffixes.

**Assert:**

- All classes implementing `IDomainEvent` must end in `DomainEvent`.
- All classes implementing `IIntegrationEvent` must end in `IntegrationEvent`.

### TC-02: Should_ResideInContractsProject_ForIntegrationEvents

**Scenario:** Integration events must be physically located in the `Contracts` project of their respective module to ensure visibility and separation.

**Arrange:**

- Scan `Contracts` project assemblies.

**Act:**

- Check the namespace and project path of all `IIntegrationEvent` implementations.

**Assert:**

- Any class implementing `IIntegrationEvent` MUST exist within a `Contracts` project.

### TC-03: Should_BeImmutableAndLogicFree_ForEventContracts

**Scenario:** Integration events must be `sealed record` types without any business logic to guarantee contract purity.

**Arrange:**

- Inspect `IIntegrationEvent` implementations via reflection.

**Act:**

- Check for `sealed` modifier, `record` type, and absence of methods.

**Assert:**

- Events are `sealed records`.
- Events contain no methods or non-data properties.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that no internal IDs (leaking DB internals) are exposed in the contract structure if they are not part of the public domain fact.

### 3.2 Observability & Traceability

- Ensure contracts are pure and do not attempt to implement cross-cutting concerns like `IInboxMessage` or carry transport-level identifiers.

### 3.3 Contract Stability

- Automated snapshot testing should be used to detect any change in the public surface area of integration events (preventing accidental breaking changes).

---

## 4. Test Data

- **Inputs:**
  - Classes implementing `IIntegrationEvent` / `IDomainEvent`.
  - Project assemblies scan result.

- **Expected Outputs:**
  - Compliance with naming (Past Tense).
  - Compliance with physical location (`Contracts` project).
  - Compliance with contract structure (`sealed record`).

---

## 5. TDD Execution Plan

1. **RED**
   - Introduce a new integration event that intentionally violates the naming or location rules.
   - Run existing architecture tests to observe failures.

2. **GREEN**
   - Correct the event implementation to match the ADR-0030 standards.
   - Update architecture test rules if new event types are introduced.

3. **REFACTOR**
   - Ensure the architecture test suite itself is DRY and scalable for new modules.

---

## 6. Exit Criteria

- [ ] All Architecture tests pass (Naming, Location, Structure).
- [ ] No Integration event references in Domain layer.
- [ ] All integration events verified as `sealed record`.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── ArchitectureTests/
 │   └── Hector.ArchitectureTests/
 │       ├── IntegrationEventNamingTests.cs
 │       ├── IntegrationEventLocationTests.cs
 │       └── EventContractArchitectureTests.cs
 └── UnitTests/
     └── Hector.Modules.Projects.Contracts.UnitTests/
         └── EventContractPurityTests.cs
```

## Summary

This test plan ensures that [ADR-0030](/docs/adr/0030-event-naming-and-contract-stability-rules.md) is enforced via automated architectural guards. By validating these rules early in the CI pipeline, we maintain the integrity of our modular boundaries and the stability of integration event contracts, which is essential for the long-term success of the `Hector` modular monolith.
