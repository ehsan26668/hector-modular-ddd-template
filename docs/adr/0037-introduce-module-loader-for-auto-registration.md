# ADR-0037: Introduce ModuleLoader for Automatic Module Registration

## Status

Proposed

## Context

After ADR-0019 (StronglyTypedId assembly scanning), the system now supports:

- Automatic discovery of strongly typed id assemblies
- Composite assembly providers
- Reflection-based registration patterns

However, module registration in the application composition root is still manual.

Currently, each module requires explicit registration of:

- Application layer services
- Mediator handlers
- Infrastructure services
- DbContext
- StronglyTypedId assembly providers

As the number of modules grows, manual registration introduces:

- Boilerplate
- Risk of missed registrations
- Increased coupling in Program.cs
- Reduced scalability of the Modular Monolith pattern

To evolve the template into a production-grade modular monolith, a standardized module bootstrap mechanism is required.

---

## Decision

Introduce a `ModuleLoader` responsible for:

1. Discovering all feature modules automatically.
2. Registering:
   - Application services
   - Mediator handlers
   - Infrastructure services
   - DbContexts
   - StronglyTypedId providers
3. Enforcing module boundaries through reflection-based scanning.
4. Providing a single extension method:

```csharp
builder.Services.AddModules(configuration);
```

Each module will expose a minimal contract, for example:

```csharp
public interface IModule
{
    void Register(IServiceCollection services, IConfiguration configuration);
}
```

Or alternatively, a marker-based scanning strategy:

```csharp
IModuleAssemblyMarker
```

The final design will favor:

- Convention over configuration
- Zero manual wiring per module
- Strict layering rules
- Deterministic startup behavior

---

## Consequences

### Positive

- Eliminates manual module wiring
- Improves scalability of Modular Monolith
- Reduces startup composition errors
- Aligns with production-grade .NET modular architectures
- Enables future dynamic module loading
- Simplifies template adoption

### Negative

- Additional reflection at startup
- Slight increase in architectural complexity
- Requires strong architectural tests to prevent abuse

---

## Architectural Principles Reinforced

- High cohesion per module
- Low coupling between modules
- Convention-driven infrastructure
- Explicit boundaries
- Self-contained feature modules

---

## Future Work

- Define ModuleLoader contract shape
- Add architecture guard tests
- Evaluate performance impact of scanning
- Consider dynamic module enable/disable support
- Possibly introduce ModuleManifest metadata

---

## Related ADRs

- ADR-0017: Standardize Feature Module Structure
- ADR-0019: Simplify StronglyTypedId and Use Assembly Scanning
- ADR-0020: One DbContext per Feature Module
- ADR-0036: Architecture Guard Tests
