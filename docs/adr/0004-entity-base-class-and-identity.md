# ADR 0004: Entity Base Class and Identity

## Status

Accepted

## Context

In Domain-Driven Design, entities are objects defined by their identity rather than their attributes. Multiple entities may share the same attribute values while still representing different domain concepts due to their unique identity.

Without a shared base abstraction, entity implementations across the system may duplicate identity logic, equality checks, and domain behavior. This can lead to inconsistent implementations and subtle bugs when comparing entities.

To enforce consistency and reduce duplication, the system requires a standard base class for all domain entities.

## Decision

We will introduce a base Entity class inside the Building Blocks domain layer.

All domain entities will inherit from this base class to standardize identity handling and equality semantics.

The base class will:

- Expose a strongly typed identifier
- Implement equality comparison based on identity
- Provide a consistent foundation for domain entities

Entities are considered equal when their identifiers are equal and they belong to the same concrete type.

## Consequences

Positive:

- Consistent identity handling across the domain
- Reduced duplication of equality logic
- Clear semantic distinction between entities and value objects
- Easier implementation of aggregates and domain behaviors

Negative:

- All domain entities must inherit from the base class
- Slight abstraction overhead for very simple entities
