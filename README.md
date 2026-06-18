# Hector — Modular DDD .NET Template

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
- High test coverage
- Architecture Decision Records (ADR)

The project is designed as a **reference architecture and starter template** for modular monoliths and systems that may evolve toward distributed architectures.

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
- Development Workflow
- Contribution Guide
- Future Improvements

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

---

### Explicit Boundaries

The system is divided into **feature modules**.

Each module owns:

- its domain model
- its application logic
- its persistence configuration

Modules communicate through **contracts**, not direct coupling.

---

### Thin Application Layer

Application handlers orchestrate use cases but **do not contain business logic**.

Handlers typically:

- create or load aggregates
- invoke domain behavior
- persist changes

---

### Persistence Ignorance

Domain models have **no dependency on EF Core**.

Persistence concerns are implemented in the **Persistence layer**.

---

### Testable Architecture

All building blocks are designed to be easily tested through:

- unit tests
- integration tests
- architecture tests

---

## System Architecture

The system follows a **layered modular architecture**.

```
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
Background Processor
        │
        ▼
Mediator.PublishAsync
        │
        ▼
Inbox Pattern
        │
        ▼
Integration Event Handler
```

Feature modules sit on top of the shared framework building blocks.

---

## Project Structure

```
src/
 ├─ Framework/
 │
 │   ├─ Hector.BuildingBlocks.Domain
 │   ├─ Hector.BuildingBlocks.Application
 │   └─ Hector.BuildingBlocks.Persistence
 │
 └─ Modules/
     └─ Projects
         ├─ Domain
         ├─ Application
         ├─ Infrastructure
         └─ Contracts
```

Tests are organized separately:

```
tests/
 ├─ UnitTests
 └─ IntegrationTests
```

---

## Core Building Blocks

The architecture is composed of three core building block libraries.

### BuildingBlocks.Domain

Contains domain modeling primitives:

- Entity
- AggregateRoot
- ValueObject
- DomainEvent
- DomainException
- StronglyTypedId

---

### BuildingBlocks.Application

Contains application layer abstractions:

- ICommand
- IRequest
- ICommandHandler
- IRequestHandler
- IMediator
- Pipeline behaviors

---

### BuildingBlocks.Persistence

Contains persistence infrastructure:

- Base DbContext
- Domain event dispatcher
- Strongly typed ID converters
- Assembly scanning utilities

---

## Domain Layer

The domain layer contains the **core business model**.

### Entity

Entities represent objects with identity.

```
Entity<TId>
```

Equality is determined by identity rather than full state.

---

### Aggregate Root

Aggregates define **consistency boundaries**.

```
AggregateRoot<TId>
```

Aggregates:

- enforce invariants
- manage internal state
- raise domain events

Domain events are stored internally:

```
private readonly List<IDomainEvent> _domainEvents
```

Available methods:

```
RaiseDomainEvent()
GetDomainEvents()
ClearDomainEvents()
```

Event ordering is preserved by insertion order.

---

### Value Objects

Value objects represent immutable domain concepts defined purely by their values.

Characteristics:

- immutable
- structural equality
- no identity

---

### Domain Exceptions

Domain rules are enforced through explicit exceptions.

```
DomainException
BusinessRuleViolationException
```

---

### Guard Pattern

Domain invariants are protected through guard methods.

Example:

```
Ensure.NotEmpty(name, "Project name cannot be empty");
```

---

## Strongly Typed ID Strategy

Instead of primitive identifiers, the system uses **Strongly Typed IDs**.

Example:

```
public sealed class ProjectId : StronglyTypedId<ProjectId>
```

Benefits:

- prevents ID mixups
- eliminates primitive obsession
- improves domain expressiveness
- improves compile‑time safety

---

## EF Core Integration

Strongly typed IDs are mapped automatically using:

```
StronglyTypedIdValueConverter
CompositeStronglyTypedIdAssemblyProvider
StronglyTypedIdRegistrationExtensions
```

Assembly scanning registers converters automatically.

---

## Application Layer

The application layer implements **CQRS using an internal mediator**.

Core abstractions:

```
ICommand
IRequest
ICommandHandler
IRequestHandler
IMediator
IPipelineBehavior
```

---

### Command Flow

```
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

Handlers coordinate use cases but **do not contain business rules**.

---

### Pipeline Behaviors

Pipeline behaviors allow cross‑cutting concerns.

Example:

```
ValidationBehaviorf
```

Additional behaviors may implement:

- logging
- authorization
- transactions
- metrics

---

## Persistence Layer

Persistence is implemented using **EF Core**.

Central component:

```
HectorDbContext
```

Each module provides its own DbContext.

Example:

```
ProjectsDbContext : HectorDbContext
```

This enables **one DbContext per feature module**.

---

## Domain Event Pipeline

Domain events are automatically dispatched during persistence.

Pipeline sequence:

```
1. Aggregates raise domain events
2. DbContext collects events
3. State changes are persisted
4. Domain events are dispatched
5. Events are cleared
```

Implementation location:

```
HectorDbContext.SaveChangesAsync()
```

Dispatcher:

```
DomainEventDispatcher
```

Publishing mechanism:

```
mediator.PublishAsync(domainEvent)
```

Events are dispatched sequentially.

---

## Feature Module Architecture

Each feature module contains four layers.

```
Module
 ├─ Domain
 ├─ Application
 ├─ Infrastructure
 └─ Contracts
```

Example:

```
Projects
 ├─ Domain
 │   ├─ Project
 │   ├─ ProjectId
 │   └─ ProjectCreatedDomainEvent
 │
 ├─ Application
 │   └─ CreateProjectCommand
 │
 ├─ Infrastructure
 │   ├─ ProjectsDbContext
 │   └─ ProjectRepository
 │
 └─ Contracts
```

Modules are designed to be **independently evolvable**.

---

## End‑to‑End Command Flow

Example use case: **Create Project**.

```
CreateProjectCommand
        ↓
CreateProjectCommandHandler
        ↓
Project.Create(name)
        ↓
ProjectCreatedDomainEvent raised
        ↓
ProjectRepository.AddAsync
        ↓
DbContext.SaveChangesAsync
        ↓
DomainEventDispatcher
        ↓
Mediator.Publish
```

---

## Testing Strategy

The template includes several types of tests.

### Domain Unit Tests

Validate domain logic and invariants.

Examples:

```
AggregateRootTests
EntityTests
ProjectTests
```

---

### Application Tests

Validate mediator behavior and pipeline execution.

Examples:

```
MediatorTests
ValidationBehaviorTests
```

---

### Persistence Tests

Verify EF Core integration and event dispatch behavior.

Examples:

```
HectorDbContextTests
StronglyTypedIdMappingTests
```

---

### Module Integration Tests

Test complete application flows.

Example:

```
CreateProjectTests
```

These tests validate:

```
Command → Handler → Repository → DbContext → Database
```

SQLite in‑memory is used as the test database.

---

## Architecture Decision Records

All major architectural decisions are documented as ADRs.

Examples:

```
0005 Domain Events
0008 Strongly Typed IDs
0013 Base DbContext and Domain Event Dispatch Strategy
0014 Internal Mediator for CQRS
0016 Domain Event Dispatch in EF Save Pipeline
0020 One DbContext per Feature Module
```

---

## Trade‑offs

### Simplicity vs Reliability

Domain events currently use **commit‑first dispatch**.

Advantages:

- simple
- minimal infrastructure
- easy to reason about

Limitations:

- not guaranteed delivery
- not suitable for distributed messaging without an outbox

---

## Extension Points

The architecture supports multiple extensions.

Possible additions:

- transactional outbox
- event bus integration
- distributed modules
- saga orchestration
- background event processing
- module isolation per database

---

## Development Workflow

Typical development flow:

1. Model domain aggregate
2. Implement domain behavior
3. Raise domain events
4. Write unit tests
5. Create command handler
6. Implement repository
7. Write integration test

---

## Contribution Guide

When contributing to the architecture:

- follow the existing module structure
- write unit tests for domain logic
- document architectural changes using ADRs
- avoid placing business logic inside handlers
- keep modules loosely coupled

---

## Future Improvements

Potential enhancements:

- transactional outbox
- distributed event bus
- module communication contracts
- background event processing
- multi‑tenant support
- observability instrumentation

---

## Summary

Hector provides a robust foundation for building domain‑driven systems in .NET.

Core strengths:

- strongly modeled domain
- modular architecture
- mediator‑based application layer
- automatic domain event dispatch
- strongly typed identifiers
- extensive test coverage

The template prioritizes **clarity, maintainability, and architectural discipline** while remaining lightweight enough for real‑world projects.
