# Testing Standards

## Testing Frameworks

The project uses the following testing libraries:

- **Unit Testing:** xUnit
- **Assertion Library:** FluentAssertions
- **Mocking:** NSubstitute
- **Architecture Testing:** NetArchTest.Rules

These tools provide a lightweight and expressive testing stack that integrates well with .NET projects.

## Naming Convention

Tests follow the naming pattern:

Should_ExpectedBehavior_When_StateUnderTest

Example:

    Should_ReturnTrue_When_ValueObjectsAreEqual

This convention makes tests self‑descriptive and easy to understand.

## Test Types

The solution contains several categories of tests.

Unit Tests

- focus on domain logic
- verify business rules
- test entities, value objects, and domain services
- avoid external dependencies

Integration Tests

- verify infrastructure behavior
- test database integration
- validate API endpoints
- ensure correct wiring between components

Architecture Tests

- enforce architectural constraints
- validate dependency rules between layers
- ensure modules respect defined boundaries

## Testing Strategy for Domain Models

Domain models are the heart of the system. Testing them rigorously is crucial to prevent regressions.

Value Objects
Focus on value equality and immutability.
    Should_BeEqual_When_PropertiesAreSame
    Should_NotBeEqual_When_PropertiesAreDifferent

Aggregates
Test invariants (business rules) and Domain Event collection.
    Should_ThrowDomainException_When_InvariantIsViolated
    Should_RecordDomainEvent_When_StateChanges

Domain Events
Ensure events are correctly captured within the AggregateRoot during state transitions.
    Should_AddDomainEvent_ToCollection_When_ActionIsPerformed

Architecture Rules
Use NetArchTest to enforce dependency rules automatically.
    Should_NotReference_Infrastructure_FromDomain
    Should_NotReference_Application_FromDomain

## TDD Workflow

The project encourages a Test‑Driven Development workflow.

Red
Write a failing test that describes the expected behavior.

Green
Implement the minimal code required to make the test pass.

Refactor
Improve the code structure while keeping all tests green.

This cycle helps maintain high code quality and ensures the domain model evolves safely.
