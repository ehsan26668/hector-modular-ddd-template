# ADR 0009: Strongly Typed IDs Enhancement

## Status

Accepted

## Context

In ADR 0008, we introduced basic `StronglyTypedId<TValue>`. However, the current implementation requires manual value assignment and lacks a unified way to create new IDs (especially for `Guid` types). To improve Developer Experience (DX) and reduce boilerplate, we need a more robust base class.

## Decision

We will enhance the `StronglyTypedId` abstraction to:

1. Provide a standard way to generate new IDs (e.g., `New()`).
2. Add support for common operations like parameterless factory methods for Guids.
3. Ensure the base class remains compatible with EF Core Value Converters.

## Consequences

- **Positive**: Reduced boilerplate in domain projects.
- **Positive**: Consistent ID generation across the entire system.
- **Negative**: Slightly more complex base class logic in the Building Blocks.
