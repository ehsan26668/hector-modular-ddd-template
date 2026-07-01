# Test Plan: ADR-0040 Module-Level Registration of Integration Event Contract Assemblies for Outbox Resolution

## Status

Accepted

## Context

This test plan validates the **module-level registration of integration event contract assemblies for outbox resolution** described in [ADR-0040](/docs/adr/0040-module-level-registration-of-integration-event-contract-assemblies-for-outbox-resolution.md).  
This ADR is critical because the outbox serializer/deserializer infrastructure must resolve integration event types during message processing without violating modular boundaries. In a modular DDD architecture each module owns its Contracts project, therefore the persistence infrastructure cannot rely on a single static assembly. Instead, modules must explicitly register their contract assemblies so the resolver can locate event types during outbox deserialization.

The behavior must ensure that:

- modules can safely contribute their own contract assemblies
- multiple modules can register contracts without overriding each other
- the resolver can correctly locate event types during deserialization
- unregistered assemblies are never scanned
- incorrect or missing registrations fail in a predictable way

These guarantees are essential to maintain **modular isolation, runtime reliability, and deterministic event deserialization**.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on registry logic, assembly aggregation, resolver construction, and edge-case handling.
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Focus on dependency injection wiring, resolver behavior with real assemblies, and multi-module registration.
  - Target Project: `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

- **Included:**
  - Registration of contract assemblies using `AddOutboxEventContracts`
  - Aggregation of assemblies from multiple modules
  - Event type resolution through `IOutboxEventTypeResolver`
  - Handling duplicate assembly registration
  - Behavior when event types are missing
  - Ensuring resolver only scans registered assemblies

- **Excluded:**
  - Message broker delivery
  - Inbox message persistence
  - Domain event publishing pipeline

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_RegisterAssembly_When_AddOutboxEventContractsIsCalled

**Scenario:**  
A module registers its Contracts assembly using the provided extension method.

**Arrange:**

- Setup a `ServiceCollection`
- Add persistence building blocks
- Prepare a Contracts assembly marker

**Act:**

- Execute `AddOutboxEventContracts` with the marker assembly

**Assert:**

- Verify the registry contains the assembly
- Verify no duplicate registrations exist
- Verify the resolver receives the assembly during construction

---

### TC-02: Should_AggregateAssemblies_When_MultipleModulesRegisterContracts

**Scenario:**  
Multiple modules independently register their Contracts assemblies.

**Arrange:**

- Setup a `ServiceCollection`
- Register two different Contracts assemblies representing separate modules

**Act:**

- Build the service provider
- Resolve the contract assembly registry

**Assert:**

- Verify both assemblies are present in the registry
- Verify registration order does not override previous assemblies
- Verify the resolver is built using the full aggregated set

---

### TC-03: Should_ResolveEventType_When_EventExistsInRegisteredAssembly

**Scenario:**  
The resolver attempts to resolve an integration event that exists in one of the registered contract assemblies.

**Arrange:**

- Register a Contracts assembly containing an integration event
- Prepare serialized event metadata such as `projects.project-created`

**Act:**

- Call the resolver to resolve the event type

**Assert:**

- Verify the resolver returns the correct CLR type
- Verify the resolved type belongs to the registered assembly
- Verify the type implements `IIntegrationEvent`

---

### TC-04: Should_NotResolveEventType_When_EventAssemblyIsNotRegistered

**Scenario:**  
An event exists in an assembly that was not registered through the contract registration mechanism.

**Arrange:**

- Register only a subset of assemblies
- Prepare serialized metadata referencing an event located in an unregistered assembly

**Act:**

- Attempt to resolve the event type

**Assert:**

- Verify the resolver returns null or throws a controlled exception
- Verify no fallback scanning occurs outside registered assemblies

---

### TC-05: Should_NotDuplicateAssembly_When_SameAssemblyRegisteredMultipleTimes

**Scenario:**  
A module accidentally calls `AddOutboxEventContracts` multiple times using the same assembly.

**Arrange:**

- Setup a `ServiceCollection`
- Register the same Contracts assembly twice

**Act:**

- Build the service provider
- Resolve the contract assembly registry

**Assert:**

- Verify the registry contains only a single instance of the assembly
- Verify duplicate registrations do not affect resolver behavior

---

### TC-06: Should_ResolveEventsFromMultipleModules_When_MultipleAssembliesRegistered

**Scenario:**  
Events exist across multiple modules and must be resolved by the same resolver.

**Arrange:**

- Register two different Contracts assemblies from different modules
- Prepare event metadata for each module

**Act:**

- Resolve event types for both events

**Assert:**

- Verify the resolver correctly resolves both event types
- Verify the returned CLR types belong to their respective assemblies

---

### TC-07: Should_FailGracefully_When_EventNameIsInvalid

**Scenario:**  
The resolver receives malformed or invalid event metadata.

**Arrange:**

- Register valid contract assemblies
- Prepare invalid event name such as `invalid-event-format`

**Act:**

- Attempt to resolve the event type

**Assert:**

- Verify the resolver fails gracefully
- Verify the system does not throw unexpected runtime exceptions
- Verify a structured error or null result is returned

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that no sensitive information, stack traces, connection strings, or internal IDs leak to external layers or clients.
- Verify that internal-only fields are not exposed through contracts or serialized payloads.

### 3.2 Observability & Traceability

- Verify that logging, metrics, and correlation/trace IDs are preserved and correctly propagated across boundaries.
- Verify that event resolution failures are observable through structured logs.

### 3.3 Contract Stability

- Verify that API or event contracts remain stable and predictable for consumers.
- Verify that contract names and event identifiers remain deterministic.

---

## 4. Test Data

- **Inputs:**
  - `ProjectsContractsAssemblyMarker` assembly
  - `BillingContractsAssemblyMarker` assembly
  - Serialized event name `projects.project-created`
  - Serialized event name `billing.invoice-issued`
  - Invalid event name `invalid-event-format`

- **Expected Outputs:**
  - Successful registration of assemblies
  - Successful event type resolution
  - Null or controlled failure for unregistered events
  - Graceful handling of malformed event names

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED**
   - Define failing tests for assembly registration
   - Define failing tests for event type resolution
   - Define failing tests for missing assemblies and malformed events

2. **GREEN**
   - Implement assembly registry
   - Implement `AddOutboxEventContracts`
   - Implement resolver construction from registered assemblies

3. **REFACTOR**
   - Simplify resolver lookup logic
   - Improve assembly scanning performance
   - Strengthen modular dependency boundaries

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Security and non-functional points are verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       └── Outbox/
 │           ├── OutboxContractRegistryTests.cs
 │           └── OutboxEventTypeResolverTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── OutboxEventResolutionTests.cs
```

## Summary

This test plan ensures that ADR-0040 is validated against the expected architectural and runtime behavior.
The result should improve system quality, reliability, and maintainability while preserving the modular boundaries defined by the architecture.
