# Hector Mediator Design Specification

## Overview

This document describes the design of the internal mediator implementation used by the Hector.BuildingBlocks.Application framework.

The mediator is responsible for dispatching application requests such as commands and queries to their corresponding handlers while enabling cross‑cutting concerns through pipeline behaviors.

The goal of this mediator is to provide a lightweight, predictable, and framework‑controlled alternative to external libraries such as MediatR.

The mediator is implemented as part of the internal application framework to ensure long‑term architectural stability and full control over the request processing pipeline.

---

### Design Goals

The mediator implementation must satisfy the following goals:

- Minimal API surface
- Clear CQRS semantics
- Full control over pipeline behaviors
- Seamless integration with dependency injection
- Support for asynchronous execution
- Support for cancellation tokens
- Zero dependency on external mediator libraries

The mediator must remain lightweight and avoid unnecessary abstraction complexity.

---

### Architectural Scope

The mediator belongs to the following layer:

    Hector.BuildingBlocks.Application

It is considered part of the internal application framework used by all modules.

The mediator is responsible for:

- Dispatching commands
- Dispatching queries
- Executing pipeline behaviors
- Resolving handlers via dependency injection

The mediator is not responsible for:

- Domain event dispatching
- Integration messaging
- Infrastructure concerns

---

### Implementation Phases

The mediator implementation will be developed in two phases.

---

### Phase 1 — Core CQRS Mediator

Phase 1 introduces the core request/handler dispatch mechanism required to implement CQRS.

Scope of Phase 1:

- Command dispatching
- Query dispatching
- Handler resolution
- Pipeline behaviors
- Cancellation token support

Phase 1 explicitly excludes application events or notifications.

This keeps the initial implementation simple and focused.

---

### Phase 2 — Application Events

Phase 2 will introduce support for application events or notifications.

These represent events that may have zero or multiple handlers.

Examples include:

- sending emails
- publishing integration events
- triggering asynchronous workflows

Unlike commands and queries, application events may have multiple subscribers.

This phase will be documented in a future ADR.

---

### Core Abstractions

The mediator is built around a small set of abstractions.

Base request abstraction:

    IRequest<TResponse>

CQRS request types:

    ICommand<TResponse>
    IQuery<TResponse>

Handler abstraction:

    IRequestHandler<TRequest, TResponse>

CQRS handler aliases:

    ICommandHandler<TCommand, TResponse>
    IQueryHandler<TQuery, TResponse>

Mediator interface:

    IMediator

Pipeline abstraction:

    IPipelineBehavior<TRequest, TResponse>

Utility type:

    Unit

---

### IRequest

IRequest represents a request that expects a response.

    public interface IRequest<out TResponse>
    {
    }

Both commands and queries derive from this abstraction.

---

### ICommand

Commands represent operations that modify application state.

    public interface ICommand<out TResponse> : IRequest<TResponse>
    {
    }

Commands may return values such as identifiers or results.

Examples:

    CreateOrderCommand : ICommand<Guid>
    DeleteCustomerCommand : ICommand<Unit>

---

### IQuery

Queries represent read‑only operations that return data.

    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }

Queries must not modify system state.

Example:

    GetOrderByIdQuery : IQuery<OrderDto>

---

### IRequestHandler

Handlers process requests and return responses.

    public interface IRequestHandler<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(
            TRequest request,
            CancellationToken cancellationToken = default);
    }

All request handlers are asynchronous.

---

### CQRS Handler Aliases

To maintain readability and reinforce CQRS semantics, two specialized handler interfaces are provided.

Command handler:

    public interface ICommandHandler<in TCommand, TResponse>
        : IRequestHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
    }

Query handler:

    public interface IQueryHandler<in TQuery, TResponse>
        : IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
    }

These interfaces do not introduce additional behavior but improve readability.

---

### IMediator

The mediator provides a single entry point for dispatching requests.

    public interface IMediator
    {
        Task<TResponse> SendAsync<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default);
    }

This method resolves the corresponding handler and executes the request pipeline.

Example usage:

    var result = await mediator.SendAsync(command);

---

### Unit Type

Commands that do not produce meaningful results should return Unit.

    public readonly struct Unit
    {
        public static readonly Unit Value = new();
    }

Example command:

    public sealed record DeleteUserCommand(Guid Id)
        : ICommand<Unit>;

---

### Pipeline Behaviors

Pipeline behaviors enable cross‑cutting concerns to be implemented independently from application logic.

Examples include:

- validation
- logging
- transactions
- performance monitoring
- authorization

Pipeline abstraction:

    public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

    public interface IPipelineBehavior<in TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> HandleAsync(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken = default);
    }

Pipeline behaviors wrap the execution of the request handler.

Execution flow example:

    ValidationBehavior
        ↓
    LoggingBehavior
        ↓
    TransactionBehavior
        ↓
    RequestHandler

---

### Pipeline Execution Order

Pipeline behaviors are executed in the order they are registered in the dependency injection container.

Example registration:

    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

Execution order:

    Validation → Logging → Transaction → Handler

---

### Dependency Injection

Handlers and pipeline behaviors are resolved via the application dependency injection container.

The mediator implementation must rely on IServiceProvider to resolve:

- request handlers
- pipeline behaviors

The mediator itself should be registered as a singleton or scoped service depending on the application configuration.

---

### Result Pattern Consideration

The mediator intentionally does not enforce a specific result pattern.

Applications may choose to return:

    ICommand<Guid>
    ICommand<Result<Guid>>

The mediator remains agnostic to the response type.

This design keeps the mediator simple and avoids coupling the framework to a specific result abstraction.

---

### File Structure

The mediator implementation should be organized as follows:

    Hector.BuildingBlocks.Application
     └── Messaging
         ├── IRequest.cs
         ├── ICommand.cs
         ├── IQuery.cs
         ├── IRequestHandler.cs
         ├── ICommandHandler.cs
         ├── IQueryHandler.cs
         ├── IMediator.cs
         ├── IPipelineBehavior.cs
         ├── RequestHandlerDelegate.cs
         └── Unit.cs

---

### Example Usage

Command:

    public sealed record CreateCustomerCommand(
        string Name,
        string Email)
        : ICommand<Guid>;

Command handler:

    public sealed class CreateCustomerCommandHandler
        : ICommandHandler<CreateCustomerCommand, Guid>
    {
        public async Task<Guid> HandleAsync(
            CreateCustomerCommand command,
            CancellationToken cancellationToken)
        {
            return Guid.NewGuid();
        }
    }

Query:

    public sealed record GetCustomerByIdQuery(Guid Id)
        : IQuery<CustomerDto>;

Query handler:

    public sealed class GetCustomerByIdQueryHandler
        : IQueryHandler<GetCustomerByIdQuery, CustomerDto>
    {
        public async Task<CustomerDto> HandleAsync(
            GetCustomerByIdQuery query,
            CancellationToken cancellationToken)
        {
            return new CustomerDto(query.Id, "Test");
        }
    }

Mediator usage:

    var result = await mediator.SendAsync(command);
