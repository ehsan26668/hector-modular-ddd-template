# Decision Log

| ID | Title | Status | Date |
| --- | --- | --- | --- |
| ADR-0001 | [Adopt Architecture Decision Records](/docs/adr/0001-adopt-architecture-decision-records.md) | Accepted | 2026-06-03 |
| ADR-0002 | [Initialize Modular Monolith Structure](/docs/adr/0002-initialize-project-structure.md) | Accepted | 2026-06-03 |
| ADR-0003 | [Adopt TDD for Building Blocks](/docs/adr/0003-adopt-tdd-for-building-blocks.md) | Accepted | 2026-06-03 |
| ADR-0004 | [Entity Base Class and Identity Strategy](/docs/adr/0004-entity-base-class-and-identity.md) | Accepted | 2026-06-03 |
| ADR-0005 | [Introduce Domain Events](/docs/adr/0005-domain-events.md) | Accepted | 2026-06-03 |
| ADR-0006 | [Domain Exception Hierarchy](/docs/adr/0006-domain-exceptions.md) | Accepted | 2026-06-03 |
| ADR-0007 | [Guard Pattern for Domain Invariants](/docs/adr/0007-guard-pattern-for-domain-invariants.md) | Accepted | 2026-06-03 |
| ADR-0008 | [Use Strongly Typed IDs](/docs/adr/0008-strongly-typed-ids.md) | Accepted | 2026-06-03 |
| ADR-0009 | [Strongly Typed IDs Enhancement](/docs/adr/0009-strongly-typed-ids-enhancement.md) | Accepted | 2026-06-03 |
| ADR-0010 | [Advanced Capabilities for Strongly Typed IDs](/docs/adr/0010-advanced-strongly-typed-id-capabilities.md) | Accepted | 2026-06-03 |
| ADR-0011 | [Eliminate Boilerplate in Strongly Typed IDs Using Self-Referencing Generics](/docs/adr/0011-eliminate-boilerplate-in-strongly-typed-ids-using-self-referencing-generics.md) | Superseded | 2026-06-03 |
| ADR-0012 | [Automated Persistence Mapping for Strongly Typed IDs](/docs/adr/0012-automated-persistence-mapping-for-strongly-typed-ids.md) | Accepted | 2026-06-03 |
| ADR-0013 | [Base DbContext and Domain Event Dispatch Strategy](/docs/adr/0013-base-dbcontext-and-domain-event-dispatch-strategy.md) | Accepted | 2026-06-03 |
| ADR-0014 | [Adopt Internal Mediator for CQRS](/docs/adr/0014-adopt-internal-mediator-for-CQRS.md) | Accepted | 2026-06-03 |
| ADR-0015 | [Implement Mediator-Based Domain Event Dispatcher in Persistence](/docs/adr/0015-implement-mediator-based-domain-event-dispatcher.md) | Accepted | 2026-06-03 |
| ADR-0016 | [Integrate Domain Event Dispatching with EF Core Save Pipeline](/docs/adr/0016-integrate-domain-event-dispatching-with-ef-core-save-pipeline.md) | Accepted | 2026-06-03 |
| ADR-0017 | [Standardize Feature Module Structure](/docs/adr/0017-standardize-feature-module-structure.md) | Accepted | 2026-06-03 |
| ADR-0018 | [Domain Identity Generation Policy](/docs/adr/0018-domain-identity-generation-policy.md) | Accepted | 2026-06-03 |
| ADR-0019 | [Simplify StronglyTypedId and Use Assembly Scanning](/docs/adr/0019-simplify-strongly-typed-id-and-use-assembly-scanning.md) | Accepted | 2026-06-03 |
| ADR-0020 | [Adopt One DbContext per Feature Module](/docs/adr/0020-adopt-one-dbcontext-per-feature-module.md) | Accepted | 2026-06-07 |
| ADR-0021 | [Adopt Transactional Outbox for Domain Event Publishing](/docs/adr/0021-adopt-transactional-outbox-for-domain-events.md) | Accepted | 2026-06-07 |
| ADR-0022 | [Implement Outbox Background Processor](/docs/adr/0022-outbox-background-processor.md) | Accepted | 2026-06-07 |
| ADR-0023 | [Adopt Inbox Pattern for Idempotent Event Handling](/docs/adr/0023-adopt-inbox-pattern-for-idempotent-event-handling.md) | **Implemented** | 2026-06-13 |
| ADR-0024 | [Adopt Distributed Locking for Outbox Processor](/docs/adr/0024-adopt-distributed-locking-for-outbox-processor.md) | **Implemented** | 2026-06-13 |
| ADR-0025 | [Introduce Outbox Cleanup and Retention Policy](/docs/adr/0025-outbox-cleanup-and-retention-policy.md) | **Implemented** | 2026-06-13 |
| ADR-0026 | [Define Event Serialization Strategy](/docs/adr/0026-event-serialization-strategy.md) | **Implemented** | 2026-06-14 |
| ADR-0027 | [Domain Event to Integration Event Bridge](/docs/adr/0027-domain-event-to-integration-event-bridge.md) | **Implemented** | 2026-06-14 |
| ADR-0028 | [Integration Event Bus Abstraction](/docs/adr/0028-integration-event-bus-abstraction.md) | Superseded | 2026-06-14 |
| ADR-0029 | [Integration Event Versioning Strategy](/docs/adr/0029-integration-event-versioning-strategy.md) | Accepted | 2026-06-14 |
| ADR-0030 | [Event Naming and Contract Stability Rules](/docs/adr/0030-event-naming-and-contract-stability-rules.md) | **Implemented** | 2026-06-15 |
| ADR-0031 | [Event Schema Evolution Strategy](/docs/adr/0031-event-schema-evolution-strategy.md) | Proposed | 2026-06-07 |
| ADR-0032 | [Event Metadata and Correlation Strategy](/docs/adr/0032-event-metadata-and-correlation-strategy.md) | Proposed | 2026-06-07 |
| ADR-0033 | [Event Ordering and Delivery Guarantees](/docs/adr/0033-event-ordering-and-delivery-guarantees.md) | Proposed | 2026-06-07 |
| ADR-0034 | [Dead Letter and Poison Message Handling](/docs/adr/0034-dead-letter-and-poison-message-handling.md) | Proposed | 2026-06-07 |
| ADR-0035 | [Consumer Idempotency Strategy](/docs/adr/0035-consumer-idempotency-strategy.md) | Proposed | 2026-06-07 |
| ADR-0036 | [Architecture Guard Tests](/docs/adr/0036-architecture-guard-tests.md) | Accepted | 2026-06-07 |
| ADR-0037 | [Introduce ModuleLoader for Automatic Module Registration](/docs/adr/0037-introduce-module-loader-for-auto-registration.md) | Proposed | 2026-06-07 |
| ADR-0038 | [Enforce Pure Transactional Outbox and Remove Immediate Domain Event Dispatch](/docs/adr/0038-enforce-pure-transactional-outbox-and-remove-immediate-domain-event-dispatch.md) | Accepted | 2026-06-07 |
| ADR-0039 | [Separate Integration Event from Inbox Message](/docs/adr/0039-separate-integration-event-from-inbox-message.md) | Accepted | 2026-06-14 |
| ADR-0040 | [Module-Level Registration of Integration Event Contract Assemblies for Outbox Resolution](/docs/adr/0040-module-level-outbox-event-contract-registration.md) | Accepted | 2026-06-15 |
