# Hector — Modular DDD .NET Template

![.NET](https://img.shields.io/badge/.NET-10-blue)
![Architecture](https://img.shields.io/badge/Architecture-DDD-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

Hector is a **modular Domain‑Driven Design (DDD) architecture template for .NET** designed for building scalable and maintainable enterprise systems.

It provides a clean architectural foundation with:

- Modular architecture
- Domain‑Driven Design
- CQRS with an internal mediator
- Strongly Typed IDs
- Domain Events
- Transactional Outbox
- Inbox Pattern (Idempotent Consumers)
- Background Event Processing
- EF Core integration
- Architecture tests
- High test coverage
- Architecture Decision Records (ADR)

The project is designed as a **reference architecture and starter template** for modular monoliths that can evolve toward distributed systems.

---

## Quick Start

1. Click **"Use this template"** on GitHub
2. Clone your new repository
3. Run:

```bash
dotnet restore
dotnet build
dotnet test
```

1. Run the host:

```bash
dotnet run --project src/Host/Hector.Host
```

---

## Table of Contents

- Vision
- Architectural Principles
- System Architecture
- Project Structure
- Core Building Blocks
- Domain Layer
- Application Layer
- Persistence Layer
- Domain Event & Messaging Pipeline
- Transactional Outbox
- Inbox Pattern
- Strongly Typed ID Strategy
- Feature Module Architecture
- End‑to‑End Flow
- Testing Strategy
- Architecture Decision Records
- Trade‑offs
- Extension Points

---

## Vision

The goal of Hector is to provide a **clean, explicit, and highly maintainable architecture** for complex business systems.

Key objectives:

- Protect business rules inside the domain model
- Keep infrastructure concerns isolated
- Enable modular evolution of the system
- Provide strong type safety
- Ensure architectural decisions are documented

Hector is optimized for **modular monolith architectures that can evolve toward microservices**.

---

## Architectural Principles

### Domain First

Business logic lives inside **aggregates and value objects**, not in application services.

The domain layer is the **source of truth for business rules**.

### Explicit Boundaries

The system is divided into **feature modules**.

Each module owns:

- its domain model
- its application logic
- its persistence configuration

Modules communicate through **events and contracts**, not direct coupling.

### Thin Application Layer

Application handlers orchestrate use cases but **do not contain business logic**.

Typical responsibilities:

- load or create aggregates
- invoke domain behavior
- persist changes

### Persistence Ignorance

Domain models have **no dependency on EF Core**.

Infrastructure concerns live in the **Persistence layer**.

### Testable Architecture

All building blocks are designed for:

- unit tests
- integration tests
- architecture tests

---

## System Architecture

```text
Application Layer
        │
        ▼
Domain Layer
        │
        ▼
Persistence Layer
        │
        ▼
Transactional Outbox
        │
        ▼
Outbox Processor
        │
        ▼
Integration Event Bus
        │
        ▼
Inbox Pattern
```

---

## Project Structure

```text
src/
 ├─ Framework/
 │   ├─ Hector.BuildingBlocks.Domain
 │   ├─ Hector.BuildingBlocks.Application
 │   ├─ Hector.BuildingBlocks.Persistence
 │   └─ Hector.BuildingBlocks.Web
 │
 ├─ Host/
 │   └─ Hector.Host
 │
 └─ Modules/
     └─ Projects
         ├─ Domain
         ├─ Application
         ├─ Infrastructure
         └─ Contracts
```

Tests:

```text
tests/
 ├─ UnitTests
 ├─ IntegrationTests
 └─ ArchitectureTests
```

---

## Core Building Blocks

### BuildingBlocks.Domain

Domain primitives:

- Entity
- AggregateRoot
- ValueObject
- DomainEvent
- DomainException
- StronglyTypedId

### BuildingBlocks.Application

Application abstractions:

- ICommand
- IQuery
- IRequest
- ICommandHandler
- IQueryHandler
- IRequestHandler
- IMediator
- IPipelineBehavior
- ModuleLoader

### BuildingBlocks.Persistence

Infrastructure components:

- Base DbContext
- Domain event dispatcher
- Outbox processor
- Inbox store
- Strongly typed ID converters

### BuildingBlocks.Web

Web infrastructure:

- Correlation middleware
- Request correlation propagation
- HTTP integration support

---

## Domain Layer

The domain layer contains the **core business model**.

### Entity

```csharp
Entity<TId>
```

Entities are compared by identity.

### Aggregate Root

```csharp
AggregateRoot<TId>
```

Responsibilities:

- enforce invariants
- manage state
- raise domain events

### Value Objects

Characteristics:

- immutable
- structural equality
- no identity

### Domain Exceptions

```text
DomainException
BusinessRuleViolationException
```

### Guard Pattern

```csharp
Ensure.NotEmpty(name, "Project name cannot be empty");
```

---

## Strongly Typed ID Strategy

Example:

```csharp
public sealed class ProjectId : StronglyTypedId<ProjectId>
```

Benefits:

- prevents ID mixups
- eliminates primitive obsession
- improves domain expressiveness

---

## EF Core Integration

Strongly typed IDs are mapped automatically using:

- StronglyTypedIdValueConverter
- CompositeStronglyTypedIdAssemblyProvider
- StronglyTypedIdRegistrationExtensions

---

## Application Layer

The application layer implements **CQRS with an internal mediator**.

```csharp
ICommand
IQuery
IMediator
ICommandHandler
IQueryHandler
IPipelineBehavior
```

### Command Flow

```text
Command
  ↓
Mediator
  ↓
Pipeline Behaviors
  ↓
Command Handler
  ↓
Repository
  ↓
DbContext SaveChanges
```

---

## Persistence Layer

Persistence uses **EF Core**.

Base context:

```csharp
HectorDbContext
```

Each module has its own context.

Example:

```csharp
ProjectsDbContext : HectorDbContext
```

---

## Domain Event & Messaging Pipeline

```text
1. Aggregate raises domain event
2. DbContext collects events
3. State changes saved
4. Events written to Outbox
5. Outbox processor publishes events
```

---

## Transactional Outbox

Ensures reliable event delivery.

```text
Domain Event
   ↓
OutboxMessage
   ↓
Outbox Processor
   ↓
Integration Event Bus
```

Benefits:

- reliable event publishing
- no lost events
- eventual consistency support

---

## Inbox Pattern

Provides **idempotent event processing**.

Each processed message is recorded.

```text
MessageId + Consumer
```

Duplicate messages are ignored.

---

## Feature Module Architecture

```text
Module
 ├─ Domain
 ├─ Application
 ├─ Infrastructure
 └─ Contracts
```

Example:

```text
Projects
 ├─ Domain
 ├─ Application
 ├─ Infrastructure
 └─ Contracts
```

---

## End‑to‑End Flow

Example: **Create Project**

```text
CreateProjectCommand
      ↓
CommandHandler
      ↓
Aggregate.Create()
      ↓
DomainEvent
      ↓
Outbox
      ↓
Outbox Processor
```

---

## Testing Strategy

### Unit Tests

Validate domain logic.

Examples:

- AggregateRootTests
- EntityTests
- ProjectTests

### Integration Tests

Validate EF Core and messaging pipeline.

Examples:

- HectorDbContextTests
- OutboxProcessorTests

### Architecture Tests

Verify structural rules.

Examples:

- LayerDependencyTests
- ModuleLayerRulesTests
- QueryHandlerSideEffectTests

---

## Architecture Decision Records

Architectural decisions are documented in `docs/adr`.

Examples:

- Domain Events
- Strongly Typed IDs
- Transactional Outbox
- Inbox Pattern
- Module Structure

---

## Trade‑offs

Advantages:

- strong architectural boundaries
- explicit domain modeling
- reliable messaging

Costs:

- additional infrastructure complexity
- more abstractions compared to CRUD architectures

---

## Extension Points

Possible future additions:

- external message brokers
- distributed modules
- saga orchestration
- read model projections
- multi‑tenant support
- observability instrumentation

---

## Summary

Hector provides a strong foundation for **DDD‑oriented modular systems in .NET**.

Core strengths:

- strongly modeled domain
- modular architecture
- mediator‑based application layer
- transactional outbox & inbox
- strongly typed identifiers
- architecture guard tests

The template prioritizes **clarity, maintainability, and architectural discipline** while remaining practical for real‑world systems.

---

## v1.0.0 – Initial Stable Release

This is the first stable release of Hector.

Included capabilities:

- Modular Monolith Architecture
- CQRS with internal mediator
- Transactional Outbox
- Inbox pattern (idempotent consumers)
- End-to-end correlation support
- Architecture guard tests
- Strongly typed identifiers
- ADR documentation

This version is considered production-ready as a reference modular DDD template.

---

## License

This project is licensed under the MIT License.
