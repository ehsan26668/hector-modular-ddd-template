# ADR 0004: Entity Base Class and Identity

## Status

Accepted

## Context

In Domain-Driven Design (DDD), entities are defined by their identity rather than their attributes.  
Two entities may share identical attribute values while still representing different domain concepts due to distinct identities.

Without a shared abstraction, identity handling and equality logic may be inconsistently implemented across the system, leading to duplication, subtle comparison bugs, and broken aggregate semantics.

Because entities are fundamental to aggregates and domain modeling, identity behavior must be standardized at the architectural level.

The Building Blocks layer already provides strongly typed identifiers and other domain primitives.  
A consistent base Entity abstraction is required to unify identity handling across all modules.

## Decision

We introduce a base `Entity<TId>` class inside:

`Hector.BuildingBlocks.Domain/Primitives`

All domain entities across all modules MUST inherit from this base class.

The base class SHALL:

- Expose a strongly typed identifier (`TId`)
- Enforce identity-based equality semantics
- Override equality operators (`==`, `!=`)
- Implement `Equals` and `GetHashCode` based on identity
- Ensure equality only when:
  - Identifiers are equal
  - Concrete types are the same

Entities are considered equal if and only if they share the same identity and belong to the same concrete type.

Identity types SHOULD use StronglyTypedId to prevent primitive obsession and accidental cross-entity comparison.

This abstraction establishes a consistent semantic distinction between:

- Entities (identity-based equality)
- ValueObjects (structural equality)

## Consequences

Positive:

- Consistent identity semantics across the entire domain
- Elimination of duplicated equality logic
- Strong alignment with DDD tactical patterns
- Safer aggregate implementation
- Prevention of accidental cross-type identity comparison

Negative:

- All entities must inherit from the base class
- Adds a small abstraction layer even for simple entities
- Requires disciplined use of strongly typed identifiers
