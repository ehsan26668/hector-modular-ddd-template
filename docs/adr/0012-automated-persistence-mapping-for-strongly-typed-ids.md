# ADR 0012: Automated Persistence Mapping for Strongly Typed IDs

## Status

Accepted

## Context

With the introduction of `StronglyTypedIdCrtp<TSelf>` ([ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md)), we have a type-safe way to define identities. However, EF Core cannot natively map these custom types to database columns (e.g., `Guid`).

Manually defining a `ValueConverter` for every ID in every `EntityTypeConfiguration` is error-prone, violates DRY principles, and increases boilerplate. We need a centralized way to handle this mapping across all modules.

## Decision

We will implement an automated mapping mechanism in the `Hector.BuildingBlocks.Persistence` project using EF Core **Model Configuration Conventions**.

The implementation will use a generic value converter and register it globally in the `DbContext`.

    // Generic Converter
    public class StronglyTypedIdEfValueConverter<TId> : ValueConverter<TId, Guid>
        where TId : StronglyTypedIdCrtp<TId>, IStronglyTypedId<TId>
    {
        public StronglyTypedIdEfValueConverter() 
            : base(id => id.Value, value => TId.Create(value)) { }
    }

    // Convention Registration in DbContext
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<TId>()
            .HaveConversion<StronglyTypedIdEfValueConverter<TId>>();
    }

This ensures that any type inheriting from `StronglyTypedIdCrtp<TId>` is automatically recognized and mapped by EF Core without manual configuration.

## Consequences

Positive:

- **Zero Configuration**: New IDs work with persistence automatically.
- **Domain Purity**: No EF-specific code or attributes needed in the Domain layer.
- **Maintainability**: Centralized logic for all ID conversions.

Negative:

- **Library Dependency**: Requires a dependency on `Microsoft.EntityFrameworkCore` in the persistence building block.
- **Generic Complexity**: Requires careful handling of generic constraints to match the CRTP pattern.
