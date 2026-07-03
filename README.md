# Hector - Modular DDD .NET Template

![.NET](https://img.shields.io/badge/.NET-10-blue)
![Architecture](https://img.shields.io/badge/Architecture-DDD-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

Hector is a modular Domain-Driven Design (DDD) architecture template for .NET for building scalable and maintainable enterprise systems.

It provides a clean architectural foundation with:

- Modular Architecture: Explicit bounded contexts
- Domain-Driven Design: Aggregates, value objects, and domain events
- Result-Based Application Flow: Functional error handling for expected failures
- Standardized Error Taxonomy: Centralized validation, conflict, not found, and business rule handling
- CQRS: Internal mediator implementation
- Strongly Typed IDs: Type-safe identifiers to eliminate primitive obsession
- Persistence: Transactional outbox and inbox pattern for idempotent consumers
- Quality Assurance: Architecture guard tests
- Documentation: Architecture Decision Records (ADR)

## What's New in v1.1.0 (Result-Based Architecture)

We have migrated from an exception-driven flow to a functional Result pattern.

- Predictable APIs: All command and query handlers now return `Result` or `Result<T>`
- Standardized Errors: A centralized error taxonomy ensures consistent error reporting across all modules
- Web Integration: Automatic mapping of `Result` objects to appropriate HTTP status codes via `ResultEndpointFilter`

## System Architecture

```text
Application Layer (Result-Oriented)
        |
        v
Domain Layer (Exception-Free Invariants)
        |
        v
Persistence Layer
        |
        v
Transactional Outbox -> Outbox Processor -> Integration Event Bus -> Inbox Pattern
```

## Core Building Blocks

### BuildingBlocks.Application

- `Result` & `Result<T>`: Canonical response types for all application operations
- `Error` & `ErrorCategory`: Rich error objects covering Validation, NotFound, Conflict, BusinessRule, etc.
- Messaging: `ICommand`, `IQuery`, and `IMediator` with built-in Result support
- Pipeline Behaviors: Automated validation and transaction management

### BuildingBlocks.Domain

- Primitives: `Entity`, `AggregateRoot`, `ValueObject`
- Invariants: `Ensure` guard pattern for domain integrity
- Identity: `StronglyTypedId` to eliminate primitive obsession

## Application Layer & Result Pattern Example

In Hector, we avoid using exceptions for expected failures.

```csharp
public async Task<Result<ProjectId>> Handle(CreateProjectCommand request, CancellationToken ct)
{
    if (await _repository.ExistsAsync(request.Name))
    {
        return Result.Failure<ProjectId>(ProjectErrors.NameAlreadyExists);
    }

    var project = Project.Create(request.Name);
    await _repository.AddAsync(project);

    return Result.Success(project.Id);
}
```

## Testing Strategy: Architecture Tests

We use ArchUnit-style architecture tests to keep the architecture stable:

- ResultRules: Ensures all handlers return Result types
- LayerRules: Ensures Domain never depends on Application or Persistence
- ErrorContractTests: Ensures error codes follow naming conventions and remain stable

## Architecture Decision Records (ADR)

Recent architectural decisions documented in `docs/adr`:

- ADR-0047: Standardize Result Pattern
- ADR-0050: Establish Application Error Taxonomy
- ADR-0052: Introduce Result/Error Object Model
- ADR-0054: Adopt Result-based Error Handling Architecture

## License

This project is licensed under the MIT License.
