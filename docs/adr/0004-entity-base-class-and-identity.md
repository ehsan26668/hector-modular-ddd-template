# ADR 0004: Base Entity Class and Identity Strategy

## Status

Accepted

## Context

Entities require a stable identity (ID) for equality comparison, unlike Value Objects which use structural equality. We need a base class to standardize this across all modules.

## Decision

We will implement a generic `Entity<TId>` base class where:

1. `TId` is constrained to be non-nullable.
2. Equality is strictly based on the `Id` property.
3. It will support both primitive IDs (Guid, int) and Strongly Typed IDs (ValueObjects).

## Consequences

- Consistent identity management across the modular monolith.
- Simplifies repository implementations.
