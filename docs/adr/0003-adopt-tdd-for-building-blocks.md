# ADR 0003: Adopt Test-Driven Development (TDD) for Building Blocks

## Status

Accepted

## Context

The Building Blocks layer contains foundational abstractions that form the core of the architecture.  
Examples include:

- Entity
- ValueObject
- AggregateRoot
- StronglyTypedId
- Domain exceptions
- Guard utilities

These components live inside the Framework layer, primarily in:

- Hector.BuildingBlocks.Domain
- Hector.BuildingBlocks.Application
- Hector.BuildingBlocks.Persistence

Because all feature modules depend on these abstractions, any defect or incorrect design decision in this layer can propagate throughout the entire system.

Ensuring correctness, stability, and predictable behavior of these primitives is therefore critical.

## Decision

All Building Blocks components MUST be implemented using **Test-Driven Development (TDD)**.

The development workflow follows the classic TDD cycle:

1. Write a failing unit test
2. Implement the minimal code required to make the test pass
3. Refactor the implementation while keeping tests green

Unit tests must validate:

- domain invariants
- equality semantics for ValueObjects
- identity semantics for Entities
- behavior of AggregateRoot domain event handling
- StronglyTypedId correctness
- edge cases and invalid states

Each Building Block must have comprehensive unit test coverage before it is considered complete.

Tests are located in dedicated test projects such as:

- `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`
- `tests/UnitTests/Hector.BuildingBlocks.Application.UnitTests`
- `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

These tests act as executable specifications for the architectural primitives.

## Consequences

Positive:

- High confidence in the correctness of core abstractions
- Early detection of design issues
- Improved modeling discipline
- Safer long-term refactoring
- Tests serve as living documentation of expected behavior

Negative:

- Slower initial implementation speed
- Requires strong testing discipline
- Developers must be comfortable with TDD practices
