# ADR-0019: Simplify StronglyTypedId and Use Assembly Scanning

## Status

Accepted

## Context

The project uses strongly typed identifiers to avoid mixing primitive identifiers across aggregates and entities. The initial implementation used a self-referencing generic base type `(StronglyTypedId<TSelf>)` and required manual persistence configuration for each identifier type.

While this approach worked, it introduced friction in the persistence layer and made registration harder to keep modular. We needed a way to:

1. Keep type safety for identifiers.

2. Allow feature modules to expose their own identifier assemblies.

3. Register EF Core value converters automatically without manual per-type configuration.

4. Avoid circular dependencies in the dependency injection container.

During implementation, a naive registration approach that resolved `IEnumerable<IStronglyTypedIdAssemblyProvider>` from inside the `IStronglyTypedIdAssemblyProvider` factory caused a circular dependency. The final registration strategy must avoid resolving the same service collection from its own composite factory.

## Decision

We will keep strongly typed identifiers as a generic base type for now, and we will standardize persistence registration through assembly scanning and a composite assembly provider.

1. **Module-owned assembly exposure**  
   Each feature module can expose one or more IStronglyTypedIdAssemblyProvider implementations.

2. **Concrete provider registration**  
   Provider implementations are registered as concrete singleton services first.

3. **Composite provider as the public abstraction**  
   A CompositeStronglyTypedIdAssemblyProvider is registered as the single IStronglyTypedIdAssemblyProvider used by persistence infrastructure.

4. **Factory-based composition**  
   The composite provider is built from the concrete provider types discovered during registration, not by re-resolving `IEnumerable<IStronglyTypedIdAssemblyProvider>` from the same service graph.

5. **Automated EF Core conversion**  
   Persistence uses the registered assembly providers to discover strongly typed identifier types and apply `StronglyTypedIdValueConverter<TId>` automatically.

## Implementation Notes

The final registration pattern is:

```text
var providerTypes = assemblies
    .SelectMany(a => a.GetTypes())
    .Where(t =>
        typeof(IStronglyTypedIdAssemblyProvider).IsAssignableFrom(t) &&
        !t.IsAbstract &&
        !t.IsInterface &&
        t != typeof(CompositeStronglyTypedIdAssemblyProvider))
    .Distinct()
    .ToArray();

foreach (var providerType in providerTypes)
{
    services.AddSingleton(providerType);
}

services.AddSingleton<IStronglyTypedIdAssemblyProvider>(sp =>
{
    var providers = providerTypes
        .Select(providerType => (IStronglyTypedIdAssemblyProvider)sp.GetRequiredService(providerType))
        .ToArray();

    return new CompositeStronglyTypedIdAssemblyProvider(providers);
});
```

This avoids the DI cycle that occurred when the composite factory attempted to resolve the same interface it was producing.

## Consequences

### Positive

- Removes circular dependency risk in DI registration.
- Keeps strongly typed ID persistence registration modular.
- Allows each module to provide its own assemblies explicitly.
- Preserves type safety while reducing manual EF Core configuration.
- Works cleanly with the current generic strongly typed ID model.

### Negative

- Reflection is still used during registration and model setup.
- The current domain model still uses the generic strongly typed ID base type, so a future non-generic redesign would require a separate breaking refactor.
- Module authors must ensure their assembly provider types are discoverable during scanning.
