# ADR 0036: Architecture Guard Tests

## Status

Implemented

## Context

The project follows a modular architecture based on Domain‑Driven Design and Clean Architecture principles. The system is organized into feature modules and layered components such as Domain, Application, and Infrastructure.

As the codebase evolves, architectural drift may occur. Developers may accidentally introduce dependencies that violate architectural boundaries. Examples include:

- Domain layer referencing Application or Infrastructure
- Application layer referencing Infrastructure directly
- Feature modules depending on other feature modules
- Domain logic bypassing StronglyTypedId conventions
- Infrastructure concerns leaking into the domain model

While code reviews help mitigate these risks, they are not sufficient to guarantee long‑term architectural integrity.

To ensure architectural rules remain enforced as the system grows, automated architecture validation is required.

## Decision

The project will introduce an **Architecture Test Suite** that automatically verifies architectural constraints during test execution.

Architecture rules will be implemented as executable tests inside a dedicated test project:

    tests/ArchitectureTests

These tests will validate important architectural constraints such as:

    Domain layer must not reference Application layer
    Domain layer must not reference Infrastructure layer
    Application layer must not reference Infrastructure layer
    Feature modules must not depend on other feature modules
    Domain identifiers must inherit from StronglyTypedId<>
    Domain assemblies must not generate identifiers using Guid.NewGuid()

Architecture tests may use reflection and architecture inspection tools such as:

    NetArchTest.Rules
    System.Reflection

Each rule will be implemented as a test following the testing standards naming convention:

    Should_<ExpectedBehavior>_When_<Condition>

Example:

    Should_NotDependOnInfrastructure_When_InDomainLayer()

These tests will run automatically together with the rest of the test suite using:

    dotnet test

Any architectural violation will cause the test suite to fail, preventing the change from being accepted.

## Consequences

Positive:

- Prevents architectural drift over time
- Enforces architectural rules automatically
- Makes architectural boundaries explicit and verifiable
- Improves confidence during refactoring
- Documents architecture rules as executable tests

Negative:

- Architecture tests must be maintained as modules evolve
- Some violations may still require manual architectural review
- Adds a small amount of complexity to the test suite
