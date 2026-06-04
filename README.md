# Hector Modular Monolith Template

Hector is a production-ready .NET 10 starter kit designed to bootstrap enterprise-grade applications. It enforces architectural integrity through Domain-Driven Design (DDD), Clean Architecture, and a strict Modular Monolith approach.

## Key Features

- Domain-Driven Design: First-class support for Aggregates, Value Objects, and Domain Events.
- Modular Monolith: Strong module boundaries that enable future microservice extraction.
- Clean Architecture: Business logic remains independent from frameworks and infrastructure.
- CQRS: Implemented using MediatR for clear separation of commands and queries.
- TDD-First: Core building blocks are implemented using strict Test-Driven Development.
- Reliable Messaging: Outbox pattern support for reliable domain event dispatching.

## Getting Started

### Prerequisites

- .NET 10 SDK
- PostgreSQL
- Rider or Visual Studio (with `.slnx` support)

### Clone the repository

    git clone https://github.com/your-org/hector-modular-monolith-template.git
    cd hector-modular-monolith-template

### Build the solution

    dotnet build Hector.slnx

### Run tests

    dotnet test Hector.slnx

## Architecture Overview

The project follows a Clean Architecture approach combined with a Modular Monolith structure.

    Domain
      ↑
    Application
      ↑
    Infrastructure
      ↑
    Host (API / Worker)

### Module Structure

Each module follows the same internal structure:

    ModuleName
     ├── Domain
     ├── Application
     ├── Infrastructure
     └── Contracts

This ensures consistent boundaries and maintainability.

## Building Blocks

The project provides a reusable internal framework located in the `Framework` directory.

    Framework
     ├── Domain
     │   ├── Entity
     │   ├── ValueObject
     │   ├── AggregateRoot
     │   ├── StronglyTypedId
     │   └── Domain Events
     │
     ├── Application
     │   ├── CQRS abstractions
     │   └── Pipeline behaviors
     │
     ├── Persistence
     │   ├── EF Core configuration
     │   └── Outbox implementation
     │
     └── Observability
         ├── Logging
         └── Metrics

## Documentation

Project documentation is located in the `docs` folder.

- Product Vision → [product-vision.md](/docs/vision/product-vision.md)
- Architecture Overview → [architecture-overview.md](/docs/architecture/architecture-overview.md)
- Testing Standards → [testing-standards.md](/docs/standards/testing-standards.md)
- Decision Log → [decision-log.md](/docs/decisions/decision-log.md)

### Architecture Decisions (ADR)

All architectural decisions are documented using ADRs.

See:

    /docs/adr

Examples:

- 0001 – [Adopt Architecture Decision Records](/docs/adr/0001-adopt-architecture-decision-records.md)
- 0002 – [Initialize Project Structure](/docs/adr/0002-initialize-project-structure.md)
- 0003 – [Adopt TDD for Building Blocks](/docs/adr/0003-adopt-tdd-for-building-blocks.md)
- 0004 – [Entity Base Class and Identity](/docs/adr/0004-entity-base-class-and-identity.md)
- 0005 – [Domain Events](/docs/adr/0005-domain-events.md)
- 0006 – [Domain Exception Hierarchy](/docs/adr/0006-domain-exceptions.md)
- 0007 – [Guard Pattern for Domain Invariants](/docs/adr/0007-guard-pattern-for-domain-invariants.md)
- 0008 – [Strongly Typed IDs](/docs/adr/0008-strongly-typed-ids.md)

## Testing

The project follows strict testing standards.

Test types:

- Unit Tests → Domain logic and building blocks
- Integration Tests → Infrastructure behavior
- Architecture Tests → Boundary enforcement

Run all tests:

    dotnet test

Testing conventions and rules are documented in:

    /docs/testing-standards.md

## Contributing

Before contributing:

1. Follow the existing architectural boundaries.
2. Write tests before implementing domain logic (TDD).
3. Document architectural changes with a new ADR.
4. Ensure all tests pass.

## License

This project is provided as an open-source template. License details can be added depending on project requirements.
