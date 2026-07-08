# ADR 0002: Initialize Project Structure for Modular DDD

## Status

Implemented

Implemented on: 2026-07-08

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

The solution SHALL adopt a **modular project structure** organized into three primary areas:

- Framework (shared building blocks)
- Modules (feature modules)
- Host (startup and execution entry point)

### Framework (Building Blocks)

Reusable infrastructure, web abstractions, and cross-cutting concerns are placed under:

`src/Framework`

This layer provides shared architectural primitives and consists of:

- `Hector.BuildingBlocks.Domain`: Domain primitives, base entity/aggregate classes, domain event contracts, and system invariant rules.
- `Hector.BuildingBlocks.Application`: Mediator abstractions, CQRS messaging pipeline behaviors, validation, correlation handling, inbox integration points, and application result models.
- `Hector.BuildingBlocks.Persistence`: Base EF Core DbContext, strongly typed ID persistence support, inbox/outbox storage engines, domain event dispatching, and transaction pipeline behaviors.
- `Hector.BuildingBlocks.Web`: Shared HTTP middleware, correlation middleware, global exception handling, endpoint filters, and result-to-HTTP mapping infrastructure.
- `Hector.ArchitectureTests.Framework`: Internal architecture testing DSL and rule evaluation primitives used by architecture guard tests.

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

#### Responsibilities

- **Domain**: Contains aggregates, entities, value objects, domain events, repository interfaces, and core business rules with no dependency on application or infrastructure concerns.
- **Application**: Contains commands, queries, handlers, use case orchestration, error catalogs, and application-level policies.
- **Infrastructure**: Contains EF Core persistence, repository implementations, module registration, strongly typed ID assembly providers, and external integration implementations.
- **Contracts**: Contains integration events and public contracts exposed to other modules for inter-module communication.

### Host Application

The execution host resides in a unified host project under:

`src/Host/Hector.Host`

This project acts as the composition root and is responsible for:

- dependency injection bootstrapping
- module loading and registration
- middleware pipeline configuration
- application startup
- background service initialization
- environment-specific runtime configuration

### Testing Structure

Tests are organized into dedicated categories under:

```text
tests
├── ArchitectureTests
├── UnitTests
├── IntegrationTests
├── TemplateTests
└── Shared
```

- **ArchitectureTests**: Validates domain isolation, module boundaries, layer dependencies, naming conventions, event contract rules, error contract rules, and package policy enforcement.
- **UnitTests**: Validates domain logic, business rules, CQRS handlers, middleware behavior, result handling, and framework building blocks in isolation.
- **IntegrationTests**: Verifies persistence behavior, outbox/inbox flows, HTTP exception handling, module interactions, and database-backed execution paths.
- **TemplateTests**: Verifies the generated template structure and installability expectations of the packaged solution template.
- **Shared**: Contains reusable testing infrastructure such as application factories and persistence test helpers.

### Centralized Build Configuration

The solution uses centralized build and dependency configuration via:

- `Directory.Build.props`
- `Directory.Packages.props`

This ensures:

- consistent package version management
- shared compiler and analyzer settings
- reduced duplication across projects
- enforcement of Central Package Management (CPM)

## Consequences

### Positive

- Clear separation of architectural layers
- Strong modular boundaries
- Reusable building blocks across modules
- Centralized host composition
- Dedicated architecture test support
- Improved maintainability and scalability
- Easier test isolation and verification
- Better consistency in package and build management

### Negative

- Higher initial structural complexity
- Requires strong architectural discipline to preserve boundaries
- Increased number of projects and solution maintenance overhead
- Internal DSL and guard test framework add maintenance responsibility to the template itself
`
