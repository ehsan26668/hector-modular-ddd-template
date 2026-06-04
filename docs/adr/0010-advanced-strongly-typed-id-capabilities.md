# ADR 0010: Advanced Capabilities for Strongly Typed IDs

## Status

Accepted

## Context

The project already uses strongly typed identifiers to enforce type safety and improve expressiveness in the domain model.

As the system evolves, identifiers frequently need to be created, parsed from external input (such as APIs or persistence layers), or represented as an empty value. Without standardized helper capabilities, these operations introduce repetitive boilerplate and inconsistent implementations across the codebase.

Providing a small set of standardized utilities for strongly typed identifiers will simplify identifier handling and ensure consistent behavior across the domain, application, and infrastructure layers.

## Decision

We will extend the strongly typed identifier abstraction with a small set of convenience capabilities focused primarily on Guid-based identifiers.

These capabilities will standardize common identifier operations such as creation, parsing, and safe conversion from external values.

The following helper APIs will be supported for strongly typed identifiers:

    Example:
    OrderId.New();
    OrderId.Empty;
    OrderId.Parse(string value);
    OrderId.TryParse(string value, out OrderId id);

These capabilities will be implemented while preserving the existing behavior of the strongly typed identifier abstraction and keeping the domain model strongly typed.

## Consequences

Positive:

- Reduces repetitive boilerplate when creating and handling identifiers.
- Standardizes identifier creation and parsing across the solution.
- Improves usability in APIs, application services, and persistence layers.
- Maintains strong type safety while improving developer ergonomics.

Negative:

- Slightly increases the surface area of the base abstraction.
- Requires additional tests and careful design to avoid unnecessary complexity.
