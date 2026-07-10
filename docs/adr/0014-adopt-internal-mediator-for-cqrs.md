# ADR 0014: Adopt Internal Mediator for CQRS

## Status

Accepted

## Context

The application layer implements the CQRS pattern to separate commands (state-changing operations) from queries (read operations).

A mediator component is required to dispatch commands and queries to their corresponding handlers while enabling cross‑cutting concerns such as validation, logging, and transaction management through pipeline behaviors.

A common approach in .NET applications is to use the MediatR library for this purpose. However, introducing an external mediator library creates additional dependency management and reduces control over core architectural behavior.

Since the mediator pattern is conceptually simple and the project already provides internal building blocks, implementing a lightweight internal mediator provides better architectural control and long‑term stability.

The goal is to provide a minimal, predictable mediator abstraction that fits naturally within the Hector.BuildingBlocks.Application framework.

## Decision

The system will implement an internal mediator instead of relying on MediatR or any external mediator library.

A lightweight mediator implementation will be introduced in the Hector.BuildingBlocks.Application project.

The mediator will be responsible for dispatching commands and queries to their corresponding handlers and supporting pipeline behaviors for cross‑cutting concerns.

The mediator abstraction will include the following core interfaces:

    ICommand
    ICommandHandler<TCommand>

    IQuery<TResult>
    IQueryHandler<TQuery, TResult>

    IMediator

    IPipelineBehavior<TRequest, TResponse>

The mediator will be integrated with the dependency injection container and used by the application layer to coordinate command and query execution.

Pipeline behaviors will allow the framework to implement concerns such as validation, logging, and transaction handling without polluting application or domain logic.

## Consequences

Positive:

- Removes dependency on external mediator libraries
- Provides full architectural control over request dispatching
- Enables custom pipeline behaviors tailored to the framework
- Keeps the application layer lightweight and predictable
- Aligns with the internal BuildingBlocks framework philosophy

Negative:

- Requires internal implementation and maintenance
- Slight increase in framework complexity
- Future improvements must be implemented internally instead of relying on external library updates.
