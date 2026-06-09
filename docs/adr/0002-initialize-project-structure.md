# ADR 0002: Initialize Project Structure for Modular DDD

## Status

Accepted

## Context

The project is designed to follow Domain-Driven Design (DDD) with a modular architecture.

Without a well-defined structure from the beginning, the codebase may quickly become tightly coupled, difficult to maintain, and resistant to change.

To maintain long-term architectural integrity, the solution must enforce:

- clear domain boundaries
- separation of concerns
- modular scalability
- testability
- strict dependency direction

The project structure should reflect both **DDD tactical patterns** and **Clean Architecture layering** while remaining practical for real-world .NET development.

## Decision

The solution SHALL adopt a **modular project structure** organized into two primary areas:

- Framework (shared building blocks)
- Modules (feature modules)

### Framework (Building Blocks)

Reusable infrastructure and abstractions are placed under:

`src/Framework`

This layer provides shared architectural primitives and consists of:

- `Hector.BuildingBlocks.Domain`
- `Hector.BuildingBlocks.Application`
- `Hector.BuildingBlocks.Persistence`

These projects provide reusable capabilities such as:

- domain primitives
- mediator abstractions
- CQRS messaging contracts
- persistence infrastructure
- outbox and inbox reliability mechanisms

### Feature Modules

Business functionality is implemented as **independent feature modules** located under:

`src/Modules/<FeatureName>`

Each module follows Clean Architecture layering:

```text
<Feature>
├── Domain
├── Application
├── Infrastructure
└── Contracts
```

### Responsibilities

**Domain**  
Contains aggregates, entities, value objects, domain events, and repository interfaces.

**Application**  
Contains commands, queries, handlers, and application orchestration logic.

**Infrastructure**  
Contains EF Core persistence, repository implementations, and external integrations.

**Contracts**  
Contains integration events and public contracts used by other modules.

### Testing Structure

Tests are organized separately under:

```text
tests
├── UnitTests
└── IntegrationTests
```

Unit tests validate domain logic and application behavior.  
Integration tests verify persistence, infrastructure behavior, and module interaction.

### Centralized Build Configuration

The solution uses centralized build and dependency configuration via:

- `Directory.Build.props`
- `Directory.Packages.props`

This ensures consistent dependency management and build settings across the entire solution.

## Consequences

Positive:

- Clear separation of architectural layers
- Strong modular boundaries
- Improved maintainability and scalability
- Easier test isolation
- Reusable architectural building blocks

Negative:

- Higher initial structural complexity
- Requires architectural discipline to maintain boundaries
- Slightly increased solution size due to multiple projects
