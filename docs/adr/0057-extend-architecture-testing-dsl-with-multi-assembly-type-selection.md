# ADR 0057: Extend Architecture Testing DSL With Multi-Assembly Type Selection

## Status

Accepted

## Context

The initial Architecture Testing DSL introduced in ADR-0056 provided a fluent and expressive API for defining architecture rules. However, the type selection API only supported a single assembly:

- `That()` → default to Hector.BuildingBlocks.Domain
- `That(Assembly)` → single assembly

This limitation prevented the DSL from supporting essential rule categories:

- Layer isolation rules operating on multiple assemblies (e.g., DomainAssemblies)
- Module isolation rules spanning module-level assembly groups
- Convention Packs such as `LayerIsolation()` or `DomainPurity()`
- Cross-assembly rules required for dependency validation
- CQRS conventions applied over a set of feature assemblies

As a result:

- Several architecture tests were forced to bypass the DSL and fall back to raw NetArchTest syntax.
- Rule definitions became inconsistent and repetitive.
- The intent of architectural policies was hidden behind mechanical NetArchTest calls.
- The DSL could not yet act as a unified governance layer.

To enable scalable architecture governance, the DSL must support multi-assembly selection natively.

## Decision

Extend the Architecture Testing DSL with a new method:

    ITypeFilter That(IEnumerable<Assembly> assemblies);

This change enables rule authors to evaluate architectural constraints across an entire set of assemblies—especially for layered boundaries and module groups.

Implementation detail:

    public ITypeFilter That(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return new TypeFilter(Types.InAssemblies(assemblies));
    }

Scope of the enhancement:

- All layered architecture tests must migrate to the multi-assembly API.
- Convention Packs may now rely on multi-assembly selection as a first-class concept.
- Future rule composers (dependency graph analysis, rule chaining) will assume the existence of this API.

The existing DSL remains backwards-compatible and no breaking changes are introduced.

## Consequences

Positive:

- Enables cross-assembly rules required for layer isolation and module boundaries.
- Convention Packs such as `LayerIsolation()` become fully implementable.
- Test classes no longer need to work directly with raw NetArchTest.
- Architectural intent becomes clearer and DSL usage becomes consistent.
- Establishes the necessary foundation for future dependency graph traversal and rule composition.

Negative:

- Slight increase in DSL complexity.
- Existing architecture tests require one-time refactoring to use the new API.
- DSL maintainers must ensure future APIs remain coherent with multi-assembly semantics.
