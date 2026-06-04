# ADR 0007: Use Guard Pattern for Domain Invariants

## Status

Accepted

## Context

Domain models must protect their invariants to ensure the system always remains in a valid state.

Without explicit validation at the domain boundary, invalid data may enter entities, value objects, or aggregates. This can lead to inconsistent domain behavior and difficult-to-trace bugs.

Scattering validation logic across constructors and methods also leads to duplication and reduces readability. A consistent mechanism is required to express domain preconditions clearly and concisely.

## Decision

We will adopt the Guard Pattern to enforce domain invariants.

A centralized utility class named Ensure will provide guard methods used throughout the domain layer to validate inputs and protect invariants.

These guards will be used inside constructors, factory methods, and domain behaviors to prevent invalid states.

Example:

    Ensure.NotNull(value, nameof(value));
    Ensure.NotEmpty(name, nameof(name));
    Ensure.NotDefault(id, nameof(id));

When a rule is violated, the guard will throw a domain exception.

## Consequences

Positive:

- Clear and consistent validation across the domain
- Reduced duplication of validation logic
- Improved readability of domain code
- Strong protection of domain invariants

Negative:

- Additional abstraction layer for simple validations
- Developers must follow the guard usage convention consistently
