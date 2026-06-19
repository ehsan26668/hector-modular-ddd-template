# ADR-0046: Framework and Product Co‑Evolution Strategy

## Status

Accepted

## Context

Hector is intended to evolve into a production‑ready architectural framework for building modular monolith systems using DDD, CQRS, Outbox, and Inbox patterns.

To ensure that the framework evolves in a practical and validated way, a real product will be developed using Hector.

The product will be an Accounting system.

This introduces two parallel development tracks:

1. Framework development (Hector)
2. Product development (Accounting)

Without a clear strategy, these two tracks could become tightly coupled or diverge in inconsistent ways.

## Decision

Development will follow a Framework/Product Co‑Evolution model.

Two independent repositories will be maintained:

Framework repository:

```text
Hector
```

Product repository:

```text
Accounting
```

Hector will contain:

- architectural building blocks
- infrastructure patterns
- template structure
- reusable framework components

Accounting will contain:

- domain-specific business logic
- accounting modules
- APIs and product features

Accounting will always be created from the Hector template.

When Accounting discovers missing capabilities in Hector, the following workflow will be used:

1. A gap is identified while implementing a feature in Accounting.
2. An issue is created in the Hector repository.
3. Hector is updated to support the capability.
4. The template version is incremented.
5. Accounting upgrades or integrates the improvement.

This ensures that Hector evolves based on real-world requirements.

## Consequences

### Positive

- Framework evolves based on real product needs
- Architectural decisions are validated in practice
- Prevents speculative framework design
- Encourages reusable and generic abstractions

### Negative

- Requires coordination between two repositories
- Framework evolution may temporarily block product features

### Rules

The following rules apply:

1. Hector must remain domain-agnostic.
2. Business logic must never be placed in Hector.
3. Accounting must not directly modify Hector code.
4. Improvements must flow through Hector first.

### Outcome

This strategy ensures that Hector becomes a robust and production‑tested architectural framework while Accounting evolves as a real-world system built on top of it.
