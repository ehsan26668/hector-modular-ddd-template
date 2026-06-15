# Architecture Overview

## Vision

This repository provides a reusable template for building enterprise‑grade .NET applications based on the following architectural principles:

- Domain‑Driven Design (DDD)
- Clean Architecture
- Modular Monolith
- CQRS
- Transactional Outbox & Inbox
- Test‑Driven Development (TDD)

The goal of this project is to provide a production‑ready modular monolith template that enables developers to bootstrap new systems with a consistent architecture, strong domain modeling capabilities, and reliable inter‑module communication.

The template focuses on:

- maintainability
- scalability
- testability
- strict architectural boundaries
- reusable internal building blocks
- reliability in messaging

---

## Architectural Style

The system follows a **Modular Monolith** architecture.

Each module:

- encapsulates its own Domain, Application, and Infrastructure layers
- owns its data (one DbContext per module)
- communicates via integration events and contracts
- follows Clean Architecture internally

The architecture supports gradual evolution toward microservices.

---

## Solution Structure

    src
     ├── Framework
     │   ├── Hector.BuildingBlocks.Domain
     │   ├── Hector.BuildingBlocks.Application
     │   └── Hector.BuildingBlocks.Persistence
     │
     ├── Modules
     │   └── Projects
     │       ├── Domain
     │       ├── Application
     │       ├── Infrastructure
     │       └── Contracts
     │
     └── Hosts
         ├── Api
         └── Worker

    tests
     ├── UnitTests
     └── IntegrationTests

Notes:

- There is NO standalone Messaging building block.
- There is NO standalone Infrastructure building block.
- There is NO standalone Observability building block.
- Messaging abstractions are part of `Hector.BuildingBlocks.Application`.

---

## BuildingBlocks Framework

The `Hector.BuildingBlocks` projects form a lightweight internal framework shared across modules.

They provide:

- Domain primitives
- Application abstractions (Mediator + CQRS)
- Persistence and reliability infrastructure

The goal is to ensure consistency, reduce duplication, and enforce architectural rules.

---

## Architectural Layers

Each module internally follows Clean Architecture:

    Domain
      ↑
    Application
      ↑
    Infrastructure
      ↑
    Host

Rules:

- Domain has no external dependencies.
- Application depends only on Domain.
- Infrastructure depends on Application and Domain.
- Hosts compose the system.

Dependencies always point inward.

---

## Domain Kernel

The Domain layer provides foundational abstractions:

    Entity<TId>
    AggregateRoot<TId>
    ValueObject
    StronglyTypedId
    IDomainEvent
    DomainEventBase
    DomainException
    Ensure

Aggregate roots collect domain events internally.
Events are dispatched after successful persistence.

The Domain layer is:

- framework‑agnostic
- persistence‑ignorant
- side‑effect free

---

## Domain Layer

Contains:

- Aggregates
- Entities
- Value Objects
- Domain Events
- Repository interfaces
- Business rules

The domain model is pure and independent of EF Core or ASP.NET.

---

## Application Layer

Defines system use cases and orchestration logic.

Responsibilities:

- orchestrate domain operations
- implement CQRS
- dispatch domain events
- publish integration events
- coordinate workflows

Typical contents:

- Commands
- Queries
- Command handlers
- Query handlers
- Domain event handlers
- Integration event abstractions
- Pipeline behaviors

CQRS is implemented via an internal mediator located in:

    Hector.BuildingBlocks.Application

Supported abstractions include:

    IMediator
    ICommand / IRequest
    ICommandHandler / IRequestHandler
    INotificationHandler
    IPipelineBehavior
    ValidationBehavior
    InboxPipelineBehavior
    IIntegrationEventBus (abstraction)
    IInboxStore (abstraction)

Messaging is considered an Application concern.

---

## Persistence & Reliability

Persistence is implemented in:

    Hector.BuildingBlocks.Persistence

Responsibilities:

- Base HectorDbContext
- EF Core integration
- StronglyTypedId mapping
- Domain event dispatching
- Transactional Outbox
- Outbox background processor
- Inbox implementation
- Distributed locking
- Cleanup policies

### One DbContext per Module

Each module owns its schema through its own DbContext.

Direct database access between modules is forbidden.

---

## Messaging Model

The system defines three messaging levels:

### 1. In‑Process Messaging

Used for commands, queries, and notifications inside a module.

Implemented via internal mediator.

### 2. Domain Events

Raised by aggregates.
Handled inside the same module.

### 3. Integration Events

Used for cross‑module communication.

Flow:

    Domain Event
        ↓
    Application Handler
        ↓
    Integration Event
        ↓
    Outbox
        ↓
    Outbox Processor
        ↓
    IIntegrationEventBus
        ↓
    Consumer (Inbox)

This ensures reliability and eventual consistency.

---

## Module Architecture

Each module follows:

    ModuleName
     ├── Domain
     ├── Application
     ├── Infrastructure
     └── Contracts

Domain  
contains aggregates, business rules, and domain events.

Application  
contains use cases, command/query handlers, and orchestration logic.

Infrastructure  
contains EF Core DbContext, repositories, and external integrations.

Contracts  
contains integration events and public contracts shared with other modules.

---

## Hosts

Hosts are composition roots.

### API Host

Responsibilities:

- expose HTTP endpoints
- authentication & authorization
- configure dependency injection
- OpenAPI
- health checks

Technology:

- ASP.NET Core
- Minimal APIs

### Worker Host

Responsible for:

- Outbox processing
- Inbox handling
- background jobs

---

## Testing Strategy

    tests
     ├── UnitTests
     └── IntegrationTests

Unit tests:

- domain logic
- application behaviors
- building blocks

Integration tests:

- EF Core mapping
- Outbox processing
- Inbox idempotency
- module interaction

Testing follows TDD principles.

---

## Cross‑Cutting Concerns

Implemented via:

- pipeline behaviors
- persistence infrastructure
- domain primitives

Currently supported:

- validation
- idempotency (Inbox)
- domain event dispatching
- transactional outbox
- integration reliability
- strongly typed ID mapping

There is no embedded logging/observability framework inside BuildingBlocks.

---

## Design Principles

- High cohesion within modules
- Strict inward dependency rule
- Explicit module boundaries
- Infrastructure independence
- Reliable messaging
- Evolution‑friendly structure

---

## Future Evolution

The architecture supports:

- extracting modules into microservices
- replacing IIntegrationEventBus with distributed brokers
- event streaming
- multi‑tenancy
- distributed caching

The modular boundaries minimize future refactoring cost.
