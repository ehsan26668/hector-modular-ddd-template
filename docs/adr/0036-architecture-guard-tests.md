# ADR 0036: Architecture Guard Tests

## Status

Proposed

## Context

The project follows a modular architecture based on Domain-Driven Design, Clean Architecture principles, and strict separation of layers.

Over time, large codebases tend to experience architectural drift. Developers may accidentally introduce unintended dependencies between layers or modules, weakening the intended architecture.

Examples of such problems include:

- Domain layer referencing Application or Infrastructure
- Application layer referencing Infrastructure directly
- Cross-module dependencies between feature modules
- Domain identifiers bypassing the StronglyTypedId abstraction
- Infrastructure concerns leaking into the domain model

Traditional code reviews and developer discipline alone are often insufficient to prevent such issues as the codebase grows.

To ensure long-term architectural integrity, automated verification is required. Architecture tests provide a way to continuously validate architectural rules during the test phase of the build process.

This approach allows the architecture to become self-protecting by automatically detecting violations whenever the test suite runs.

## Decision

The project will introduce an **Architecture Test Suite** that validates key architectural constraints of the system.

These tests will run as part of the standard test pipeline and will prevent architectural violations from being introduced into the codebase.

Architecture tests will validate rules such as:

    Domain layer must not reference Application layer
    Domain layer must not reference Infrastructure layer
    Application layer must not reference Infrastructure layer
    Feature modules must not depend on other feature modules
    Domain identifiers must inherit from StronglyTypedId<>
    Domain assemblies must not generate identifiers using Guid.NewGuid()

Architecture tests will be implemented in a dedicated test project:

    tests/ArchitectureTests

The tests will use reflection and architecture inspection libraries such as:

    NetArchTest.Rules
    System.Reflection

Each architectural rule must be expressed as a clear test case using the naming convention defined in the testing standards:

    Should_<ExpectedBehavior>_When_<Condition>

Example:

    Should_NotDependOnInfrastructure_When_InDomainLayer()

These tests serve as **automated architectural guardrails** and are executed together with the rest of the unit and integration tests using:

    dotnet test

If an architectural violation occurs, the test suite will fail and prevent the change from being accepted.

## Consequences

Positive:

- Prevents architectural drift over time
- Provides automated enforcement of architecture rules
- Makes architectural boundaries explicit and verifiable
- Increases confidence when refactoring the codebase
- Helps maintain modularity and clean architecture principles
- Documents architectural constraints in executable form

Negative:

- Requires maintaining architecture tests when new modules or layers are added
- Some architectural violations may still require manual review
- Adds a small amount of complexity to the test suite
