# ADR 0017: Standardize Feature Module Structure

## Status

Accepted

## Context

Hector is a modular monolith template intended for enterprise-grade .NET applications using Domain-Driven Design, Clean Architecture, CQRS, and reusable building blocks.

The foundational building blocks for domain modeling, persistence, strongly typed identifiers, domain events, and internal mediation are now in place. The next step is to introduce the first real vertical slice.

Without a standardized module structure, each feature module may organize domain, application, persistence, and contracts differently. This would weaken module boundaries, increase coupling between modules, and make the template harder to understand, extend, and test.

We need a consistent structure for feature modules before introducing the first vertical slice.

## Decision

We will organize each feature module as an isolated unit that follows Clean Architecture internally.

Each module will own its domain model, application use cases, infrastructure implementation, and public contracts. Modules must not depend on implementation details of other modules. Cross-module communication should happen through contracts or domain/integration events.

The default feature module structure will be:

    Modules/
      <ModuleName>/
        Domain/
          <ModuleName>.Domain.csproj
        Application/
          <ModuleName>.Application.csproj
        Infrastructure/
          <ModuleName>.Infrastructure.csproj
        Contracts/
          <ModuleName>.Contracts.csproj

The intended dependency direction is:

    Contracts
      ↑
    Domain
      ↑
    Application
      ↑
    Infrastructure

The host application composes modules at startup and wires dependencies through dependency injection.

A module may expose API endpoints through the host application or through a dedicated presentation component, but endpoint ownership must not break module boundaries.

The first feature module will use this structure to validate the architecture end-to-end through a vertical slice.

## Consequences

Positive:

- Establishes a consistent structure for all feature modules.
- Preserves modular monolith boundaries.
- Keeps each module independently understandable, testable, and evolvable.
- Supports Clean Architecture inside each module.
- Enables future extraction of modules into services if needed.
- Provides a clear foundation for the first vertical slice.
- Reduces architectural drift as the project grows.

Negative:

- Introduces more projects and folders than a flat structure.
- Requires discipline to avoid leaking implementation details between modules.
- Adds some upfront ceremony before implementing business features.
- May feel heavier for very small modules or simple CRUD features.
