# ADR 0008: Strongly Typed IDs

## Status

Accepted

## Context

Using primitive types such as Guid, int, or string for entity identifiers can lead to a problem known as Primitive Obsession.

For example, different identifiers like OrderId, CustomerId, and ProductId may all be represented as Guid. This makes it possible to accidentally pass the wrong identifier type to a method or constructor, causing subtle bugs that the compiler cannot detect.

To improve type safety and better express domain concepts, identifiers should be represented as explicit domain types rather than primitive values.

## Decision

Entity identifiers will be implemented using Strongly Typed ID Value Objects.

Each identifier will be represented as its own type that wraps the underlying primitive value.

**Excample:**

CustomerId will wrap a Guid value instead of directly using Guid.

These identifiers will behave like Value Objects and support equality comparison.

**Excample:**

CustomerId customerId = new CustomerId(Guid.NewGuid());

Entities will then use these types instead of primitive IDs.

**Excample:**

```text
Entity<CustomerId>
```

## Consequences

* **Positive:** Prevents mixing identifiers from different aggregates.
* **Positive:** Improves type safety across the domain model.
* **Positive:** Eliminates primitive obsession for entity identifiers.
* **Negative:** Slightly more verbose code due to additional ID types.
