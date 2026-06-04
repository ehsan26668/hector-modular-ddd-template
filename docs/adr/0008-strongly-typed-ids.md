# ADR 0008: Use Strongly Typed IDs

## Status

Accepted

## Context

Using primitive types such as Guid, int, or string for entity identifiers can lead to accidental misuse. For example, it is possible to pass a CustomerId where an OrderId is expected if both are represented as Guid.

This weak typing reduces type safety and increases the risk of subtle bugs, especially in large systems with many aggregates.

To improve type safety and better express domain intent, identifiers should be modeled as dedicated domain types rather than primitive values.

## Decision

We will introduce Strongly Typed IDs for entity identifiers.

A base abstraction named StronglyTypedId will be implemented to encapsulate the underlying identifier value. Specific identifier types will derive from this base type to represent identities for different aggregates.

Example conceptual identifiers:

    OrderId
    CustomerId
    ProductId

These identifiers wrap the underlying primitive value (typically Guid) while providing strong typing at compile time.

Entities and aggregates will use these types instead of primitive identifiers.

## Consequences

Positive:

- Stronger compile-time type safety
- Prevents accidental identifier misuse across aggregates
- Improves expressiveness of the domain model
- Better alignment with DDD tactical patterns

Negative:

- Additional types must be created for each entity identifier
- Requires mapping configuration when working with persistence frameworks such as Entity Framework
