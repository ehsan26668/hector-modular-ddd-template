# Test Plan: ADR-0006 Domain Exception Hierarchy

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0006](/docs/adr/0006-domain-exceptions.md), which introduces a dedicated domain exception hierarchy.

The goal is to ensure that all business rule violations are explicitly communicated via a typed exception system (`DomainException` and `BusinessRuleViolationException`), allowing the application layer to distinguish between business-level domain failures and unexpected system-level technical exceptions.

Consistency here is vital for maintaining clear boundaries, enabling centralized error translation (e.g., mapping to HTTP 400 Bad Request), and ensuring that domain model invariants are strictly protected.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**  Focus on the exception hierarchy inheritance, proper categorization of domain errors, and ensuring domain objects correctly trigger these exceptions.

- **Target Project:**
  - `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`

---

## 1. Scope

### Included

- `DomainException` as the base class for all domain errors.
- `BusinessRuleViolationException` for specific business invariant failures.
- Correct throwing of exceptions from Entities, ValueObjects, and Aggregates.
- Availability of relevant error details (e.g., error messages, domain context).

### Excluded

- Global error handling middleware (e.g., Web API Problem Details middleware).
- Mapping logic to HTTP status codes (this belongs to the Presentation/Infrastructure layer).
- Logging mechanisms.

---

## 2. Test Cases (Unit)

### TC-01: Should_InheritFromDomainException_When_BusinessRuleViolationExceptionIsThrown

**Scenario:**

- All domain exceptions must eventually derive from `DomainException` to allow for unified catch-blocks at the application level.

**Arrange:**

- Instantiation of a `BusinessRuleViolationException`.

**Act:**

- Check inheritance chain.

**Assert:**

- The exception is assignable to `DomainException`.

---

### TC-02: Should_ThrowBusinessRuleViolationException_When_DomainInvariantIsBroken

**Scenario:**

- Domain objects (Entities/ValueObjects) must throw the specific `BusinessRuleViolationException` when a rule is violated.

**Arrange:**

- Create an entity/value object that has a business rule (e.g., “Quantity cannot be negative”).

**Act:**

- Call the method with invalid input (e.g., -1).

**Assert:**

- Throws `BusinessRuleViolationException`.
The exception message accurately describes the broken rule.

---

### TC-03: Should_AllowCustomization_When_DomainExceptionIsDefined

**Scenario:**

- Custom domain exceptions should be able to provide detailed error context.

**Arrange:**

- Define a custom exception inheriting from `DomainException`.

**Act:**

- Throw the exception with a specific error message.

**Assert:**

- Exception properties correctly carry the message and relevant state.

---

### TC-04: Should_NotAffectSystemStability_When_DomainExceptionIsThrown

**Scenario:**

- Domain exceptions should not be confused with system faults (like `NullReferenceException`).

**Arrange:**

- Arrange a test to check for the type of exception thrown.

**Act:**

- Trigger both a business rule violation and an unintended technical error (e.g., null access).

**Assert:**

- Business failures are caught as DomainException.
- System faults are correctly handled (or propagated) separately.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that exception messages do not leak internal system state or sensitive infrastructure information to the upper layers.

The messages must be “business-safe” and suitable for eventual propagation to users if needed.

### 3.2 Observability & Traceability

Verify that the stack traces and exception types are sufficient for developers to identify *exactly* which business rule was violated during debugging.

### 3.3 Contract Stability

Verify that the exception hierarchy remains stable. Any change to the base class structure must not break the existing try-catch patterns in the Application Services.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Valid and invalid domain inputs (e.g., negative money, empty project names, future dates).
  - Domain objects (Aggregates/ValueObjects) that enforce these invariants.

- **Expected Outputs:**
  - `BusinessRuleViolationException` for invalid business actions.
  - Consistent error message format.

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Create tests that expect `BusinessRuleViolationException` when domain invariants are violated.
2. **GREEN:** Create the `DomainException` and `BusinessRuleViolationException` classes and update domain models to throw them.
3. **REFACTOR:** Ensure exception messages are descriptive and check if additional specific domain exceptions are needed to improve type safety.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] DomainException and `BusinessRuleViolationException` defined.
- [ ] Unit tests covering invariant enforcement pass.
- [ ] Clear distinction between system errors and business errors.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Domain.UnitTests/
 │       └── Exceptions/
 │           └── DomainExceptionTests.cs
 │       └── Primitives/
 │           └── AggregateInvariantTests.cs
```

---

## Summary

This test plan ensures that the domain layer maintains a rigorous error-handling strategy that is separate from technical system failures. By enforcing the use of a typed exception hierarchy, we ensure the codebase remains expressive, maintainable, and ready for centralized error translation, which is essential for a robust DDD-based modular monolith.

---
