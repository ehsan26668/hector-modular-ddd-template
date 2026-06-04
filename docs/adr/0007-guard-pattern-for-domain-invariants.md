# ADR 0007: Guard Pattern for Domain Invariants

## Status

Accepted

## Context

Domain models must enforce business invariants to keep entities and aggregates in a valid state.

Without a consistent approach, validation logic such as null checks or empty string checks can lead to repetitive boilerplate code scattered across the domain model. This reduces readability and makes the core business logic harder to understand.

Since the domain already defines a dedicated exception hierarchy (DomainException and BusinessRuleViolationException), we need a standardized mechanism to validate invariants and throw the appropriate exceptions.

## Decision

Introduce a static utility class named Ensure inside the Domain layer.

The Ensure class will provide guard methods that validate domain invariants and throw BusinessRuleViolationException when a rule is violated.

Typical guard methods include:

* Ensure.NotNull(value, message)
* Ensure.NotEmpty(value, message)
* Ensure.True(condition, message)

These guards will be used inside Entities, Value Objects, and Aggregate Roots to enforce business rules in a concise and readable manner.

**Excample:**

```text
Ensure.NotEmpty(name, "Name cannot be empty.");
```

## Consequences

* **Positive:** Improves readability of domain logic by removing repetitive validation code.
* **Positive:** Centralizes invariant validation logic.
* **Positive:** Ensures all rule violations consistently throw domain-specific exceptions.
* **Negative:** Adds a small abstraction layer that developers must consistently use instead of manual validation.
