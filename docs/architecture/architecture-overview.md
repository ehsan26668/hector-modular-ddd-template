# Architecture Overview

## Vision

This repository provides a reusable template for building enterprise-grade .NET applications based on the following architectural principles:

- Domain‑Driven Design (DDD)
- Clean Architecture
- Modular Monolith
- CQRS
- Test‑Driven Development (TDD)

The goal of this project is to provide a production‑ready application template that enables developers to quickly bootstrap new systems with a consistent architecture and a solid technical foundation.

The template focuses on:

- maintainability
- scalability
- testability
- clear architectural boundaries
- reusable building blocks

## Architectural Style

The system follows a Modular Monolith architecture where each module implements Clean Architecture internally.

Key characteristics:

- Each module contains its own domain and application logic
- Modules are isolated and communicate via contracts or events
- Shared infrastructure is provided through reusable building blocks
- The architecture supports gradual evolution toward microservices if needed

## Solution Structure

The repository is organized into the following top-level directories:

```text
src
 ├── Framework
 │   ├── Hector.BuildingBlocks.Domain
 │   ├── Hector.BuildingBlocks.Application
 │   ├── Hector.BuildingBlocks.Persistence
 │   ├── Hector.BuildingBlocks.Infrastructure
 │   ├── Hector.BuildingBlocks.Messaging
 │   └── Hector.BuildingBlocks.Observability
 │
 ├── Modules
 │   └── Sample
 │       ├── Sample.Domain
 │       ├── Sample.Application
 │       └── Sample.Infrastructure
 │
 └── Hosts
     ├── Api
     └── Worker

tests
 ├── UnitTests
 ├── IntegrationTests
 └── ArchitectureTests
 ```

## BuildingBlocks Framework

The Hector.BuildingBlocks projects provide reusable abstractions and infrastructure used across modules.

These components act as an internal application framework.

They provide:

- Domain abstractions
- Application abstractions
- Persistence infrastructure
- Messaging infrastructure
- Observability components

The goal is to avoid duplication and provide consistent patterns across modules.

## Architectural Layers

Each module follows the Clean Architecture layering model.

Domain  
↑  
Application  
↑  
Infrastructure  
↑  
Host  

## Domain Kernel

The Domain layer contains the core building blocks:

```text
- Entity<TId>
- ValueObject
- AggregateRoot<TId>
- IDomainEvent
- DomainEventBase
```

Aggregate roots collect domain events internally.
Events are dispatched after persistence by the infrastructure layer.

## Domain Layer

The Domain layer contains the core business logic.

Characteristics:

- Pure domain model
- No dependency on infrastructure frameworks
- Independent from EF Core or ASP.NET

Typical contents:

- Entities
- Value Objects
- Aggregates
- Domain Events
- Domain Services
- Repository interfaces
- Business rules

## Application Layer

The Application layer defines the use cases of the system.

Responsibilities:

- orchestrate domain logic
- define commands and queries
- coordinate application workflows

Typical contents:

- Commands
- Queries
- Command handlers
- Query handlers
- DTOs
- Validators
- Pipeline behaviors

CQRS is implemented using MediatR.

## Infrastructure Layer

The Infrastructure layer provides implementations for external dependencies.

Examples:

- authentication providers
- email services
- file storage
- external APIs
- integration services

Infrastructure depends on:

- Application
- Domain

but never the other way around.

## Persistence Layer

Persistence handles data storage concerns.

This layer includes:

- EF Core DbContext
- entity configurations
- repository implementations
- Unit of Work
- database migrations
- Outbox pattern
- auditing
- soft delete

The persistence implementation is replaceable.

Default database: PostgreSQL.

## Module Architecture

Each module follows the same structure:

```text
ModuleName
 ├─ Domain
 ├─ Application
 └─ Infrastructure
```

Responsibilities:

Domain  
contains business rules and aggregates.

Application  
contains use cases and orchestration logic.

Infrastructure  
contains persistence and external integrations.

## Hosts

Hosts are entry points to the system.

Two primary hosts are defined.

## API Host

Responsibilities:

- expose HTTP endpoints
- handle authentication and authorization
- manage HTTP pipeline
- expose OpenAPI documentation
- API versioning
- health checks

Technology stack:

- ASP.NET Core
- Minimal APIs

## Worker Host

The Worker host is responsible for background processing.

Typical responsibilities:

- Outbox processing
- integration event handling
- background jobs
- scheduled tasks

## Communication Between Modules

Modules should remain loosely coupled.

Recommended communication patterns:

- application service calls
- domain events
- integration events

Direct database access between modules is not allowed.

## Testing Strategy

Testing is a core part of the architecture.

Test projects are organized as:

```text
tests
 ├─ UnitTests
 ├─ IntegrationTests
 └─ ArchitectureTests
```

Unit tests

- focus on domain logic
- test aggregates and business rules

Integration tests

- test persistence
- test API behavior

Architecture tests

- enforce dependency rules
- ensure architectural boundaries are respected

## Cross‑Cutting Concerns

Cross‑cutting concerns are implemented through BuildingBlocks.

These include:

- Logging (Serilog)
- Observability (OpenTelemetry)
- Validation
- Exception handling
- Transactions
- Authorization
- Authentication
- Auditing
- Soft delete
- Correlation IDs
- Idempotency

## Design Principles

The architecture follows several core principles.

High cohesion  
modules encapsulate their own domain logic.

Loose coupling  
modules interact via contracts and events.

Infrastructure independence  
domain logic does not depend on infrastructure frameworks.

Testability  
all layers are designed to be easily testable.

Explicit boundaries  
module boundaries are strictly enforced.

## Future Evolution

The architecture is designed to evolve.

Possible future extensions:

- microservice extraction
- distributed messaging
- event streaming
- multi-tenancy support
- distributed caching
- service mesh integration

The modular structure enables gradual evolution without rewriting the system.
