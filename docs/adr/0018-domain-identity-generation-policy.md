# ADR 0018: Domain Identity Generation Policy

## Status

Accepted

## Context

The project has adopted Strongly Typed IDs as the standard identity abstraction for domain entities (ADR‑0008 through ADR‑0011). These identifiers encapsulate Guid values and provide type safety across the domain model.

However, without a clearly defined identity generation policy, developers may accidentally generate identifiers using raw Guid APIs or bypass the intended domain identity creation mechanisms.

Examples of problematic patterns include:

- Using `Guid.NewGuid()` directly in domain entities or aggregates
- Instantiating identifiers using internal factory methods intended for infrastructure or persistence layers
- Bypassing domain conventions for identity generation

Such practices reduce consistency, weaken domain modeling standards, and may introduce architectural drift over time.

A clear identity generation policy is therefore required to ensure:

- consistent identifier generation across the domain
- enforcement of domain conventions
- protection of domain boundaries

## Decision

The domain layer must generate entity identifiers exclusively through the `StronglyTypedId` factory method intended for domain use.

Direct usage of Guid generation APIs within the domain layer is prohibited.

Identifier creation must follow the standardized pattern:

    var id = ProjectId.New();

The following practices are not allowed inside the domain layer:

    Guid.NewGuid();
    SomeId.Create(Guid.NewGuid());

Infrastructure components such as persistence mapping, deserialization, or database materialization may use specialized factory methods when reconstructing identifiers from existing values.

These factory methods are considered infrastructure concerns and must not be used by domain logic.

To prevent architectural drift, an **architecture test suite** validates that domain assemblies do not invoke `Guid.NewGuid()` directly.

This rule is automatically enforced during test execution.

Identity generation policies are therefore enforced through:

- architecture tests
- unit tests
- code review guidelines
- documented domain conventions

## Consequences

### Positive

- Establishes a clear and consistent identity generation strategy
- Prevents accidental misuse of Guid generation inside the domain layer
- Strengthens domain modeling discipline
- Protects domain boundaries from infrastructure-level concerns
- Automatically enforced by architecture tests
- Reduces the risk of architectural drift over time

### Negative

- Requires maintaining architecture tests as new domain modules are added
- Some advanced misuse scenarios could bypass detection without deeper static analysis tools
- Domain developers must follow the documented identity conventions when creating new aggregates or entities
