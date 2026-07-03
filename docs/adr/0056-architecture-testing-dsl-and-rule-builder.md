# ADR-0056: Introduce Architecture Testing DSL and Rule Builder

## Status

Accepted

## Context

The current architecture guard test suite introduced in ADR-0036 successfully validates critical architectural boundaries such as:

- Layer isolation
- Module dependency rules
- CQRS conventions
- Result pattern enforcement
- Naming conventions
- Dependency restrictions
- Event contract stability

However, the implementation currently relies heavily on low-level NetArchTest APIs and repetitive assertion patterns spread across many test classes.

As the number of architectural rules grows, several problems emerge:

- Test readability decreases.
- Rule definitions become repetitive and difficult to compose.
- Failure diagnostics are inconsistent across tests.
- Architectural policies are not expressed as reusable domain concepts.
- New contributors must understand NetArchTest internals instead of consuming a higher-level abstraction.
- Rule intent is less visible than implementation mechanics.

The current architecture tests validate the system correctly, but they do not yet provide a scalable architecture governance model.

A dedicated DSL (Domain-Specific Language) for architecture testing is needed to:

- Standardize rule declaration
- Improve readability
- Enable reusable convention packs
- Provide richer diagnostics
- Centralize architecture assertions
- Prepare the system for future Roslyn-based analysis and graph validation

This DSL must integrate naturally with the existing modular DDD architecture and testing standards adopted by Hector.

## Decision

Introduce a fluent Architecture Testing DSL and Rule Builder abstraction on top of the existing architecture testing infrastructure.

The DSL will provide expressive APIs for declaring architecture rules in a readable and composable form.

Example:

    ArchitectureRule
        .Types()
        .That()
            .ResideInNamespace("Application")
        .Should()
            .NotDependOn("Microsoft.AspNetCore")
        .Because("Application layer must remain transport-agnostic");

The initial implementation will act as:

- A composition layer
- A reusable rule abstraction
- A diagnostic enhancement wrapper around NetArchTest

The first version will NOT replace NetArchTest internals.

Instead, it will:

- Encapsulate NetArchTest usage
- Standardize assertions
- Normalize diagnostics
- Provide reusable architecture conventions

The DSL must support:

- Layer boundary rules
- Namespace rules
- Naming conventions
- Dependency restrictions
- Assembly validation
- CQRS conventions
- Result-pattern policies
- Module isolation
- Strongly typed ID rules
- Self-testing violation scenarios

The DSL will introduce reusable convention packs such as:

    Conventions.CQRS()

    Conventions.ResultPattern()

    Conventions.LayerIsolation()

    Conventions.DomainPurity()

The architecture testing infrastructure will evolve incrementally.

Phase 1:

- Fluent DSL
- Shared rule abstractions
- Unified diagnostics
- Reusable convention packs

Phase 2:

- Dependency graph traversal
- Advanced violation reporting
- Cross-module dependency analysis
- Rule composition engine

Phase 3:

- Roslyn analyzer integration
- Compile-time enforcement
- Source generator support
- IDE diagnostics

The implementation must preserve:

- Test determinism
- Fast execution time
- CI/CD compatibility
- Modular isolation
- TDD-first workflow

## Consequences

Positive:

- Architecture rules become significantly more readable.
- Rule definitions become reusable and composable.
- Failure diagnostics become standardized.
- Architecture governance becomes scalable.
- New rules can be introduced faster with less duplication.
- The architecture test suite becomes easier to maintain.
- The system gains a foundation for future Roslyn-based enforcement.
- Architectural intent becomes explicit and self-documenting.
- Teams can define convention packs aligned with DDD boundaries.

Negative:

- Additional abstraction layer introduces maintenance overhead.
- DSL design mistakes may create rigid APIs.
- Some advanced NetArchTest capabilities may initially remain exposed.
- Migration from raw NetArchTest syntax will require gradual refactoring.
- The DSL may evolve into a framework-level subsystem requiring versioning and compatibility management.
