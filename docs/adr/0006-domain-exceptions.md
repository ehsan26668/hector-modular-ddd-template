# ADR 0006: Domain Exception Hierarchy

## Status

Accepted

## Context

The domain layer must clearly distinguish between business rule violations and technical/system errors.

Using standard exceptions such as ArgumentException or InvalidOperationException does not accurately represent domain-level failures. These generic exceptions blur the boundary between technical faults and business rule violations, making error handling less expressive and harder to standardize.

A consistent exception hierarchy is required to model domain-specific failures and enable proper error translation in upper layers.

## Decision

We will introduce a dedicated domain exception hierarchy.

The design will include:

- A base class named DomainException for all domain-related errors
- A specialized exception named BusinessRuleViolationException for invariant and business rule violations

Aggregates, entities, and value objects will throw these exceptions whenever domain rules are broken.

This ensures that domain failures are explicitly modeled and can be handled consistently by the Application or Presentation layer.

## Consequences

Positive:

- Clear separation between business errors and technical failures
- Enables centralized exception handling and mapping to HTTP 400 responses
- Improves expressiveness of the domain model
- Better alignment with DDD tactical patterns

Negative:

- Requires explicit exception mapping in upper layers
- Adds additional abstraction to error handling
