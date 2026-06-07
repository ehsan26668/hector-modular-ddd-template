# ADR 0018: Domain Identity Generation Policy

## Status

Proposed

## Context

The project has adopted Strongly Typed IDs as the standard identity abstraction for domain entities (ADR‑0008 through ADR‑0011). These identifiers encapsulate Guid values and provide type safety across the domain model.

However, the current system does not explicitly define how identifiers must be generated inside the domain layer. Without a clear policy, developers may accidentally generate identifiers using raw Guid APIs or bypass the intended identity creation mechanisms.

Examples of problematic patterns include:

- Using `Guid.NewGuid()` directly in domain entities or aggregates
- Instantiating identifiers using internal factory methods intended for infrastructure or persistence layers
- Bypassing domain conventions for identity generation

Such practices reduce consistency, weaken domain modeling standards, and may introduce subtle architectural drift over time.

A clear and documented identity generation policy is therefore required to ensure:

- consistent identifier generation across the domain
- enforcement of domain conventions
- protection of domain boundaries

## Decision

The domain layer must generate entity identifiers exclusively through the `StronglyTypedId` factory method designed for domain use.

Direct usage of Guid generation APIs within the domain layer is prohibited.

Identifier creation must follow the standardized pattern:

    var id = ProjectId.New();

The following practices are not allowed inside the domain layer:

    Guid.NewGuid();
    SomeId.Create(Guid.NewGuid());

Infrastructure components such as persistence mapping or deserialization mechanisms may use specialized factory methods when reconstructing identifiers from existing values.

These factory methods are considered infrastructure concerns and must not be used by domain logic.

Architecture tests will continue enforcing dependency boundaries using **NetArchTest.Rules** as defined in the testing standards.

Identity generation policies are enforced primarily through:

- domain conventions
- unit tests
- code review guidelines

No changes to the architecture testing framework are introduced by this decision.

## Consequences

Positive:

- Establishes a clear and consistent identity generation strategy
- Prevents accidental misuse of Guid generation in the domain layer
- Strengthens domain modeling discipline
- Maintains compatibility with existing architecture testing standards
- Keeps architecture tests stable and focused on dependency rules

Negative:

- Enforcement relies partially on developer discipline and code review
- Some misuse scenarios may not be automatically detected without additional tooling such as analyzers
- Domain developers must follow the documented conventions when creating new aggregates or entities
