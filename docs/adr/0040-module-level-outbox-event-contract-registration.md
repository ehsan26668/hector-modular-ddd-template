# ADR-0040: Module-Level Registration of Integration Event Contract Assemblies for Outbox Resolution

- Status: Implemented  
- Date: 2026-06-15

## Context

The outbox serializer/deserializer infrastructure resolves integration event types by scanning assemblies for attributed event contracts.

Originally, the outbox event type resolver was registered centrally in `Hector.BuildingBlocks.Persistence` without a module-level mechanism for supplying contract assemblies. This became a problem once integration event contracts were placed in each module’s dedicated `Contracts` project, which is the intended modular DDD structure.

As a result, event types such as `projects.project-created` could not always be resolved during deserialization unless a module manually replaced the resolver registration.

A temporary workaround using `RemoveAll<IOutboxEventTypeResolver>()` and re-registering the resolver inside a module was considered, but this approach is unsafe in a modular system because one module can accidentally override registrations required by another module.

## Decision

Each module must explicitly register the assembly containing its integration event contracts through a dedicated extension method:

```csharp
services.AddOutboxEventContracts(
    typeof(ProjectsContractsAssemblyMarker).Assembly);
```

`Hector.BuildingBlocks.Persistence` must maintain a shared registry of outbox event contract assemblies and build the `IOutboxEventTypeResolver` from the complete registered set.

Modules must use an assembly marker type from the `Contracts` project rather than referencing a specific integration event type.

## Consequences

### Positive

- Preserves modular boundaries by keeping integration event contracts inside each module’s `Contracts` project
- Avoids dangerous service registration overrides such as `RemoveAll<IOutboxEventTypeResolver>()`
- Supports multiple modules contributing integration event contracts independently
- Prevents coupling infrastructure registration to a specific integration event type
- Keeps outbox serialization and deserialization aligned with modular DDD principles

### Negative

- Each module must remember to register its contract assembly
- The persistence building blocks now depend on a small registry/options mechanism for collecting assemblies

## Implementation Notes

The implementation introduces:

- an outbox event contract registry/options object
- an extension method for module-level contract assembly registration
- resolver creation based on the full set of registered assemblies
- marker-based assembly registration from each module’s `Contracts` project

Example module registration:

```csharp
services.AddHectorPersistenceBuildingBlocks();

services.AddOutboxEventContracts(
    typeof(ProjectsContractsAssemblyMarker).Assembly);
```

## Related Decisions

- ADR-0021: Adopt Transactional Outbox for Domain Event Publishing
- ADR-0026: Define Event Serialization Strategy
- ADR-0027: Domain Event to Integration Event Bridge
- ADR-0030: Event Naming and Contract Stability Rules
- ADR-0039: Separate Integration Event from Inbox Message
