# Test Plan: ADR-0029 Integration Event Versioning Strategy

## Status

Accepted

## Context

This test plan validates the **Integration Event Versioning Strategy** described in [ADR-0029](/docs/adr/0029-integration-event-versioning-strategy.md).  
This ADR is critical because the system publishes integration events through the outbox pattern, and contract identity must remain stable, explicit, and versioned without leaking transport concerns into payloads.  
The architecture relies on metadata-driven versioning via `OutboxEventAttribute` and `OutboxMessage.Version`, so the tests must prove that contract resolution, serialization, and deserialization work consistently across versions and that the payload remains free of version fields.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on contract metadata resolution, serializer behavior, message factory mapping, and failure cases for unknown or duplicate contracts.
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Focus on end-to-end outbox publishing and processing behavior, including persistence of `Type` and `Version`, and runtime resolution of integration events from stored messages.
  - Target Project: `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

- **Included:**
  - Resolution of integration event contract identity from `OutboxEventAttribute`.
  - Serialization and deserialization of integration events using the concrete CLR type.
  - Persistence of `OutboxMessage.Type` and `OutboxMessage.Version`.
  - Failure behavior for unknown contracts, missing metadata, and duplicate `(Name, Version)` registrations.
  - Event publishing flow through `IIntegrationEventBus` and outbox message creation.

- **Excluded:**
  - External broker delivery semantics.
  - Consumer-side schema compatibility testing beyond the stored contract metadata.
  - Domain event business logic unrelated to contract versioning.

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_ResolveContractIdentity_FromOutboxEventAttribute

**Scenario:** Verify that event contracts decorated with `OutboxEventAttribute` are resolved using the declared name and version.

**Arrange:**

- Create a test notification type decorated with `OutboxEvent("projects.project-created", 1)`.
- Initialize `AttributedOutboxEventTypeResolver` with the assembly containing the test type.

**Act:**

- Resolve the contract by `(Name, Version)`.

**Assert:**

- The resolver returns the correct CLR type.
- The resolved contract identity matches the declared attribute values.
- The event type does not require a `Version` property on the payload.

---

### TC-02: Should_SerializeAndDeserializeEvent_UsingConcreteClrType

**Scenario:** Verify that `SystemTextJsonOutboxEventSerializer` round-trips an integration event using the concrete CLR type resolved from metadata.

**Arrange:**

- Create a versioned integration event type decorated with `OutboxEventAttribute`.
- Create a sample event instance with representative property values.
- Initialize the serializer with an `IOutboxEventTypeResolver`.

**Act:**

- Serialize the event to JSON.
- Deserialize it back using the stored contract metadata.

**Assert:**

- The deserialized object is the same event type.
- All property values are preserved.
- No version property appears in the serialized payload.

---

### TC-03: Should_PersistTypeAndVersion_When_CreatingOutboxMessage

**Scenario:** Verify that the outbox message factory maps integration event metadata into `OutboxMessage` correctly.

**Arrange:**

- Create a test event decorated with `OutboxEventAttribute`.
- Initialize `IOutboxMessageFactory`.

**Act:**

- Create an `OutboxMessage` from the event.

**Assert:**

- `OutboxMessage.Type` matches the attribute name.
- `OutboxMessage.Version` matches the attribute version.
- `OutboxMessage.Content` contains the serialized payload of the concrete CLR type.
- No payload-level version field is introduced.

---

### TC-04: Should_Fail_When_ResolvingUnknownContract

**Scenario:** Verify that the resolver fails fast when a stored `(Type, Version)` pair cannot be matched to a known contract.

**Arrange:**

- Configure the resolver without registering the target type.
- Use a non-existent contract name/version pair.

**Act:**

- Attempt to resolve the contract.

**Assert:**

- A descriptive exception is thrown.
- The failure indicates that the contract is unknown or unsupported.

---

### TC-05: Should_RejectDuplicateContractIdentity_When_ScanningAssemblies

**Scenario:** Verify that duplicate `(Name, Version)` registrations are detected during startup scanning.

**Arrange:**

- Define two notification types with the same `OutboxEvent(Name, Version)` values.
- Initialize `AttributedOutboxEventTypeResolver`.

**Act:**

- Scan assemblies and build the contract map.

**Assert:**

- Startup fails with a clear duplicate-contract exception.
- The resolver does not silently overwrite one type with another.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that contract metadata does not expose internal implementation details beyond `Type` and `Version`.
- Verify that no sensitive fields are added to event payloads as part of versioning support.

### 3.2 Observability & Traceability

- Verify that `OutboxMessage.Type` and `OutboxMessage.Version` are written consistently for every published event.
- Verify that failures in resolution or deserialization are logged with enough context to identify the contract identity.

### 3.3 Contract Stability

- Verify that integration event contracts remain version-free at the payload level.
- Verify that backward-compatible changes introduce a new `OutboxEventAttribute` version instead of modifying payload shape in place.
- Verify that contract identity is stable as `EventName:v{Version}` across serialization and persistence.

---

## 4. Test Data

- **Inputs:**
  - `ProjectCreatedIntegrationEvent` with `OutboxEvent("projects.project-created", 1)`
  - `ProjectCreatedIntegrationEventV2` with `OutboxEvent("projects.project-created", 2)`
  - Unknown contract name/version pair
  - Duplicate contract definitions with identical metadata

- **Expected Outputs:**
  - Correct type resolution from `(Name, Version)`
  - Successful round-trip serialization and deserialization
  - Proper persistence of contract metadata in `OutboxMessage`
  - Clear failures for unknown or duplicate contracts

---

## 5. TDD Execution Plan

1. **RED**
   - Write failing tests for resolver lookup, serialization round-trip, and duplicate-contract detection.
   - Capture the expected behavior for versioned contract identity and metadata persistence.

2. **GREEN**
   - Implement or adjust `OutboxEventAttribute`, `IOutboxEventTypeResolver`, serializer, and message factory behavior to satisfy the tests.
   - Keep the payload version-free and move all version identity into metadata.

3. **REFACTOR**
   - Simplify contract scanning and resolution logic.
   - Improve exception messages, test readability, and startup validation while preserving behavior.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] `OutboxEventAttribute`-based resolution is validated.
- [ ] Serialization and deserialization round-trip correctly.
- [ ] Unknown and duplicate contract failures are covered.
- [ ] Documentation remains aligned with metadata-driven versioning.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       ├── Serialization/
 │       │   └── SystemTextJsonOutboxEventSerializerTests.cs
 │       ├── Contracts/
 │       │   └── AttributedOutboxEventTypeResolverTests.cs
 │       └── Outbox/
 │           └── OutboxMessageFactoryTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── Outbox/
             └── OutboxIntegrationEventVersioningTests.cs
```

## Summary

This test plan ensures that [ADR-0029](/docs/adr/0029-integration-event-versioning-strategy.md) is validated against the expected architectural and runtime behavior.
The result should preserve clean integration event payloads, enforce explicit contract versioning through metadata, and keep the modular DDD architecture stable and evolvable.
