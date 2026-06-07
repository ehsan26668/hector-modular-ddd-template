# ADR 0019: Simplify StronglyTypedId and Use Assembly Scanning

## Status

Accepted

## Context

Following the implementation of [ADR-0011](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md), we encountered several architectural frictions:

1. **Complexity:** The Self-Referencing Generic (CRTP) pattern created complex type hierarchies that were difficult to maintain and extend across multiple architectural layers.
2. **Persistence Coupling:** EF Core's `ConfigureConventions` could not easily map generic base classes without knowing the concrete types at startup.
3. **Boilerplate vs. Flexibility:** While CRTP reduced some boilerplate, it forced a rigid structure that made it harder for feature modules to define their own identifier logic without being heavily tied to the framework's generic constraints.

We need a simpler base structure that maintains type safety while allowing automated, modular registration for persistence.

## Decision

We will simplify the `StronglyTypedId` abstraction by removing self-referencing generics and implementing an **Assembly Scanning** mechanism for persistence mapping.

1. **Simplified Base Class:**
   `StronglyTypedId` will now be a simple abstract record/class wrapping a `Guid`, without generic self-references.

2. **Automated Discovery:**
   We will introduce `IStronglyTypedIdAssemblyProvider` to allow each module to expose its domain assemblies.

3. **Composite Registration:**
   The `HectorDbContext` will use a `CompositeStronglyTypedIdAssemblyProvider` to scan all registered assemblies and automatically apply `StronglyTypedIdValueConverter<TId>` to every concrete subclass found.

    Example Registration Logic:
    var derivedTypes = assembly.GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } &&
                    t.IsSubclassOf(typeof(StronglyTypedId)));

    foreach (var idType in derivedTypes) {
        configurationBuilder.Properties(idType)
                            .HaveConversion(typeof(StronglyTypedIdValueConverter<>).MakeGenericType(idType));
    }

## Consequences

Positive:

- **Significantly Reduced Complexity:** No more complex generic constraints (`where TSelf : StronglyTypedId<TSelf>`).
- **Truly Modular:** New modules can register their IDs by simply providing their assembly marker, adhering to the Open/Closed Principle.
- **Zero Boilerplate for Mapping:** Developers no longer need to manually configure EF Core mappings for every new ID created.
- **Improved Testability:** Simplified types are easier to mock and instantiate in unit tests.

Negative:

- **Startup Overhead:** A one-time assembly scan occurs during the first DbContext model creation (mitigated by EF Core's model caching).
- **Reflection Usage:** Uses reflection during the configuration phase to discover types, though this does not impact runtime performance after the model is built.
