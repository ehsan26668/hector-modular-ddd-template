# ADR 0011: Eliminate Boilerplate in Strongly Typed IDs Using Self-Referencing Generics

## Status

Superseded

## Context

[ADR-0008](/docs/adr/0008-strongly-typed-ids.md) introduced **Strongly Typed IDs** to improve type safety in the domain model. [ADR-0010](/docs/adr/0010-advanced-strongly-typed-id-capabilities.md) added capabilities for creation, parsing, and empty representations.

While these improved usability, they introduced **repetitive boilerplate**. Each identifier type must currently repeat similar factory and parsing methods.

Additionally, our current domain architecture heavily relies on **GUID-based identifiers**. Maintaining a generic `TValue` parameter adds unnecessary complexity to the inheritance hierarchy and complicates the base class implementation without providing immediate value for our specific use case.

The project needs a mechanism to centralize GUID-based identifier behavior while removing redundant boilerplate and simplifying the generic hierarchy.

## Decision

The project will adopt **Self-Referencing Generics (CRTP – Curiously Recurring Template Pattern)** specialized strictly for **GUID identifiers**.

A new base class structure will be introduced:

    StronglyTypedId<TSelf>
        where TSelf : StronglyTypedId<TSelf>

This design removes the `TValue` generic parameter, simplifying the hierarchy and focusing the abstraction on `Guid` as the primary identifier type.

Identifier types will follow this structure:

    public sealed class OrderId : StronglyTypedId<OrderId>
    {
        private OrderId(Guid value) : base(value) { }
        
        // Explicit factory methods for base class usage
        public static OrderId Create(Guid value) => new(value);
    }

The base class will provide common functionality shared by all GUID-based identifiers:

    TSelf New()
    TSelf Empty
    TSelf Parse(string value)
    bool TryParse(string value, out TSelf? id)

The implementation will:

- Use `Guid.CreateVersion7()` for generating new identifiers.
- Avoid reflection-based instantiation by requiring explicit factory methods in concrete types.
- Maintain compile-time type safety.

## Consequences

Positive:

- Eliminates repetitive boilerplate by centralizing GUID-specific logic.
- Simplifies the generic hierarchy by removing the `TValue` parameter.
- Provides a consistent, readable API across all domain identifiers.
- Keeps identifier creation logic centralized in the base class.

Negative:

- The abstraction is specialized for GUID identifiers and does not support other primitive types (which is acceptable for our current architecture).
- Requires concrete identifiers to expose internal factory methods for the base class to use, slightly exposing internals.
