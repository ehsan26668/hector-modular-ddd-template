# ADR 0003: Adopt Test-Driven Development (TDD) for Building Blocks

## Status

Accepted

## Context

The Building Blocks layer contains foundational domain abstractions such as Entity, ValueObject, AggregateRoot, and domain exception handling.

These components are critical to the integrity of the entire system. Any defect or incorrect assumption at this level can propagate across all modules.

To ensure correctness, stability, and long-term maintainability, we need a disciplined approach to developing these core abstractions.

## Decision

We will adopt Test-Driven Development (TDD) for implementing all Building Blocks domain components.

The development cycle will follow:

- Write a failing test
- Implement the minimal code required to pass the test
- Refactor while keeping tests green

Unit tests will:

- Validate domain invariants
- Verify equality logic (especially for ValueObjects)
- Ensure identity semantics for Entities
- Cover edge cases and invalid states

All Building Blocks must have accompanying unit tests before being considered complete.

## Consequences

Positive:

- High confidence in core abstractions
- Early detection of design flaws
- Improved domain modeling discipline
- Safer refactoring over time
- Living documentation through tests

Negative:

- Slower initial development speed
- Requires strong testing discipline
- Developers must be comfortable with TDD practices
