# ADR-0037: Introduce ModuleLoader for Automatic Module Registration

## Status

Accepted

## Context

Following the architectural decisions in:

- ADR-0017 (Standardize Feature Module Structure)
- ADR-0019 (Simplify StronglyTypedId and Use Assembly Scanning)
- ADR-0020 (One DbContext per Feature Module)

the system already relies on assembly scanning and convention-based registration for several infrastructure concerns.

However, module registration in the composition root (Host) is still manual.

Currently, each module requires explicit registration inside Program.cs for:

- Application services
- Mediator handlers
- Infrastructure services
- DbContext
- StronglyTypedId assembly providers

As the number of modules grows, this approach introduces several problems:

- Boilerplate in the composition root
- Risk of forgetting module registrations
- Increased coupling between Host and feature modules
- Reduced scalability of the Modular Monolith architecture
To maintain true modular boundaries, the Host must not have compile‑time knowledge of individual modules.

Therefore a standardized mechanism is required for automatic module discovery and registration.

---

## Decision

Introduce a ModuleLoader infrastructure responsible for automatic module discovery and registration.

Modules will expose a minimal contract:

```csharp
public interface IModuleIdentity
{
    string Name { get; }

    void Register(IServiceCollection services, IConfiguration configuration);
}
```

Each feature module must provide an implementation of this interface.

Example:

```csharp
public sealed class ProjectsModuleIdentity : IModuleIdentity
{
    public string Name => "Projects";

    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddProjectsApplication();
        services.AddProjectsInfrastructure(configuration);
    }
}
```

The system will introduce a ModuleLoader responsible for:

- Discovering implementations of IModuleIdentity
- Instantiating them
- Executing their Register method

Discovery will be implemented using reflection-based assembly scanning.

The Host will use a single extension method:

```csharp
builder.Services.AddModules(builder.Configuration);
```

The ModuleLoader will:

1. Scan loaded assemblies
2. Locate all IModuleIdentity implementations
3. Register module services automatically

This ensures the Host remains decoupled from feature modules.

---

## Consequences

### Positive

- Eliminates manual module wiring
- Keeps Host independent from feature modules
- Enables scalable Modular Monolith architecture
- Reduces startup composition errors
- Enables convention-based module onboarding
- New modules require zero changes in Host

### Negative

- Introduces reflection during application startup
- Requires clear architectural rules to avoid misuse
- Requires additional architectural tests

---

## Architectural Principles Reinforced

- Modular Monolith architecture
- Self-contained feature modules
- Low coupling between modules
- Convention over configuration
- Explicit composition root

---

## Future Work

- Implement ModuleLoader
- Add architecture tests ensuring every module exposes IModuleIdentity
- Validate module discovery behavior
- Evaluate startup performance impact
- Consider module metadata (ModuleManifest) for advanced scenarios

---

## Related ADRs

- ADR-0017 — Standardize Feature Module Structure
- ADR-0019 — Simplify StronglyTypedId and Use Assembly Scanning
- ADR-0020 — Adopt One DbContext per Feature Module
- ADR-0036 — Architecture Guard Tests
