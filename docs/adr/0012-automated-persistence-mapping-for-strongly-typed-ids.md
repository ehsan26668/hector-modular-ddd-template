# ADR-0012: Automated Persistence Mapping for Strongly Typed IDs

## Status

Accepted

## Context

With the introduction of `StronglyTypedId<TSelf>` (ADR-0011), we have a type-safe way to define identities in the domain model.

However, EF Core cannot natively map these custom identity types to primitive database column types such as `Guid`.

Manually defining a `ValueConverter` for each strongly typed ID in every `EntityTypeConfiguration` would introduce repetitive boilerplate, increase the risk of inconsistency, and violate DRY principles.

We need a centralized persistence mechanism that applies this mapping automatically across modules while keeping the domain layer free from EF Core-specific concerns.

## Decision

We will implement automated mapping for strongly typed IDs in the `Hector.BuildingBlocks.Persistence` project using EF Core model configuration.

The persistence layer will discover strongly typed ID types from configured domain assemblies and register the required EF Core value converters centrally.

The implementation uses `StronglyTypedIdValueConverter<TId>`, which converts:

- from strongly typed ID to `Guid` when writing to the database;
- from `Guid` to strongly typed ID when reading from the database.

When rehydrating an ID from the database, the converter uses reflection to invoke a non-public constructor that accepts a single `Guid` parameter.

## Implementation Contract

All strongly typed ID types must follow this contract:

1. They must inherit from `StronglyTypedId<TSelf>`.
2. They must define a private constructor that accepts a single `Guid` parameter.
3. They must expose a static factory method for creating new IDs, typically named `New`.
4. They must not rely on C# primary constructors for persistence rehydration.

Example:

```csharp
public sealed class ProjectId : StronglyTypedId<ProjectId>
{
    private ProjectId(Guid value) : base(value)
    {
    }

    public static ProjectId New()
    {
        return CreateNew(static value => new ProjectId(value));
    }
}
```

The private constructor is intentionally required. It allows the persistence layer to rehydrate IDs from database values without exposing construction logic as part of the public domain API.

Primary constructors must not be used for strongly typed IDs because the current persistence converter resolves and invokes a classic non-public constructor through reflection.

## Consequences

### Positive

- Strongly typed IDs are mapped automatically without per-entity boilerplate.
- The domain model remains free from EF Core-specific attributes and configuration.
- Mapping behavior is centralized in the persistence building block.
- New ID types can be introduced with minimal ceremony as long as they follow the constructor contract.
- The persistence contract is explicit and testable.

### Negative

- Every strongly typed ID must provide a private `Guid` constructor.
- Primary constructors are not compatible with the current rehydration mechanism.
- The persistence layer depends on reflection for ID reconstruction.
- Domain assemblies must be registered or exposed to the persistence layer for scanning.

## Validation

The behavior is covered by integration tests in `StronglyTypedIdMappingTests`.

The test `Should_PersistAndRehydrate_StronglyTypedId_When_StronglyTypedIdConventionIsConfigured` verifies that:

1. an entity with a strongly typed ID can be persisted;
2. the ID is stored as a `Guid`;
3. the entity can be rehydrated from the database;
4. the strongly typed ID value is preserved after rehydration.

## Notes

If a strongly typed ID does not provide the required private `Guid` constructor, EF Core materialization will fail during rehydration because the converter cannot reconstruct the ID instance.

Correct implementation:

```csharp
public sealed class OrderId : StronglyTypedId<OrderId>
{
    private OrderId(Guid value) : base(value)
    {
    }

    public static OrderId New()
    {
        return CreateNew(static value => new OrderId(value));
    }
}
```

Incorrect implementation:

```csharp
public sealed class OrderId(Guid value) : StronglyTypedId<OrderId>(value)
{
    public static OrderId New()
    {
        return CreateNew(static value => new OrderId(value));
    }
}
```
