# Test Plan: ADR-0031 Event Schema Evolution Strategy

## Status

Accepted

## Context

This test plan validates the **Event Schema Evolution Strategy** defined in [ADR-0031](/docs/adr/0031-event-schema-evolution-strategy.md).

The system stores serialized integration events in the transactional outbox before publication. Because these messages may remain persisted for extended periods, the architecture must guarantee that historical payloads remain deserializable and processable even as contracts evolve.

This plan ensures that event schema evolution remains safe, forward-compatible, and operationally stable by validating serialization behavior, deserialization tolerance, and version handling rules.

## Test Strategy

- **Unit Tests:**
  - Validate forward-compatible deserialization behavior.
  - Validate default handling for missing fields.
  - Validate ignoring unknown fields.
  - Validate resolver compatibility mapping for moved or renamed event types.
  - Target Project: `tests/UnitTests/[ModuleName].UnitTests`

- **Integration Tests:**
  - Validate end-to-end serialization/deserialization behavior against real outbox message format.
  - Validate that historical outbox payloads can still be read after schema evolution.
  - Target Project: `tests/IntegrationTests/[ModuleName].IntegrationTests`

- **Architecture Tests:**
  - Enforce structural rules for event contracts: immutable, data-only, stable location, no behavior.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

- **Included:**
  - Deserialization of older event payloads using newer contract versions.
  - Ignoring unknown JSON properties during deserialization.
  - Using default values for missing fields.
  - Validation that breaking changes require explicit versioning.
  - Event type resolution using outbox metadata.
  - Compatibility mapping for renamed or moved event CLR types.
  - Stable JSON contract expectations for historical outbox data.

- **Excluded:**
  - Outbox cleanup/retention policy behavior.
  - Broker-specific delivery behavior.
  - Consumer business logic unrelated to schema compatibility.

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_DeserializeOlderPayload_WhenNewOptionalFieldIsAdded

**Scenario:** A historical payload created from version 1 of an integration event must deserialize successfully into the newer event contract when only optional fields were added.

**Arrange:**

- Create a JSON payload representing `ProjectCreatedIntegrationEvent` version 1:
  - `ProjectId`
  - `Name`
- Define current event contract including:
  - `ProjectId`
  - `Name`
  - `CreatedBy` (optional)

**Act:**

- Deserialize the historical JSON payload into the current event contract.

**Assert:**

- Deserialization succeeds without exception.
- Existing fields are populated correctly.
- Newly added optional field uses default/null value.

---

### TC-02: Should_IgnoreUnknownProperties_DuringDeserialization

**Scenario:** If an outbox payload contains extra fields not present in the current event contract, deserialization must still succeed.

**Arrange:**

- Create a JSON payload containing valid event fields plus extra unknown properties such as:
  - `LegacyField`
  - `DiagnosticMetadata`

**Act:**

- Deserialize the payload into the current integration event contract.

**Assert:**

- Deserialization succeeds without exception.
- Known properties are populated correctly.
- Unknown properties are ignored.

---

### TC-03: Should_ApplyDefaultValues_WhenFieldsAreMissing

**Scenario:** Missing fields in historical messages must not cause failures when they are considered optional or defaultable in the current schema.

**Arrange:**

- Create a historical JSON payload missing one or more non-required fields.

**Act:**

- Deserialize the payload into the current integration event contract.

**Assert:**

- Deserialization succeeds.
- Missing values are assigned expected defaults.
- No invalid state is introduced by default assignment.

---

### TC-04: Should_FailContractCompatibilityCheck_WhenPropertyIsRenamedWithoutVersionBump

**Scenario:** Renaming a serialized property without introducing a new event version is a breaking change and must be detected.

**Arrange:**

- Define baseline serialized contract snapshot for an integration event.
- Simulate contract evolution where a property name changes but event version remains the same.

**Act:**

- Compare current contract metadata/snapshot against baseline.

**Assert:**

- Compatibility validation fails.
- Failure message clearly identifies renamed property as a breaking change.
- Team is forced to introduce a new versioned event contract.

---

### TC-05: Should_FailContractCompatibilityCheck_WhenPropertyTypeChangesWithoutVersionBump

**Scenario:** Changing the type of a serialized property without versioning is a breaking change and must be rejected.

**Arrange:**

- Define baseline event contract metadata.
- Change property type in the evolved contract while keeping the same event identity/version.

**Act:**

- Execute compatibility validation.

**Assert:**

- Validation fails.
- The changed property type is reported as an incompatible schema change.

---

### TC-06: Should_ResolveStoredEventType_UsingOutboxMetadata

**Scenario:** Historical outbox messages must resolve to the correct CLR contract type using persisted metadata rather than fragile runtime assumptions.

**Arrange:**

- Create an outbox message with:
  - canonical event name
  - event version
  - serialized payload

**Act:**

- Use `IOutboxEventTypeResolver` to resolve the target CLR type.

**Assert:**

- Resolver returns the expected integration event type.
- Deserialization using resolved type succeeds.

---

### TC-07: Should_UseCompatibilityMapping_WhenEventTypeHasMovedOrBeenRenamed

**Scenario:** If a previously stored event references a type that has moved or been renamed, resolver compatibility mapping must preserve readability of historical messages.

**Arrange:**

- Prepare historical metadata referencing old event identity/type mapping.
- Configure compatibility mapping in the event type resolver.

**Act:**

- Resolve event type using historical metadata.

**Assert:**

- Resolver maps old identity to the current supported CLR type.
- Historical payload remains deserializable.
- No manual migration is required for message readability.

---

### TC-08: Should_SerializeContracts_WithStableJsonShape

**Scenario:** Integration events stored in the outbox must preserve a stable JSON structure over time.

**Arrange:**

- Create an instance of a representative integration event.
- Serialize it using the production serializer configuration.

**Act:**

- Compare serialized JSON against approved baseline/snapshot.

**Assert:**

- Property names remain stable.
- Serializer output does not include accidental infrastructure-specific fields.
- Contract shape matches expected public schema.

---

### TC-09: Should_BeImmutableAndDataOnly_ForSerializableIntegrationEvents

**Scenario:** Serializable integration events must remain immutable data contracts to reduce schema evolution risk.

**Arrange:**

- Reflect over all `IIntegrationEvent` implementations.

**Act:**

- Inspect type characteristics.

**Assert:**

- Event contracts are immutable.
- Event contracts expose data only.
- No business logic methods exist on event contracts.

---

### TC-10: Should_ReadHistoricalOutboxMessages_EndToEnd

**Scenario:** Real historical outbox payloads must remain readable by the current codebase.

**Arrange:**

- Prepare representative outbox rows serialized using older schema versions.
- Use real serializer and resolver configuration.

**Act:**

- Process historical messages through the current deserialization pipeline.

**Assert:**

- Messages are successfully resolved and deserialized.
- No compatibility exceptions occur.
- Resulting event objects preserve expected business data.

---

## 3. Non-Functional Validation Points

### 3.1 Stability

- Historical outbox payloads must remain processable after contract evolution.
- Schema evolution must not introduce fragile deserialization behavior.
- Compatibility tests should run in CI to detect breaking changes early.

### 3.2 Observability

- Deserialization failures must produce diagnosable errors containing:
  - event name
  - version
  - resolver mapping details where relevant
- Compatibility mapping behavior should be observable in logs when applied.

### 3.3 Operational Safety

- Unknown fields must not break replay or reprocessing workflows.
- Missing optional fields must not create runtime instability.
- Historical messages must remain readable during deployments where multiple schema generations coexist.

### 3.4 Maintainability

- Contract evolution rules must be enforced consistently through reusable test helpers/snapshots.
- Resolver compatibility mappings should be testable and explicit.

---

## 4. Test Data

- **Inputs:**
  - Historical JSON payloads for version 1 events.
  - Evolved JSON payloads including unknown fields.
  - Outbox metadata containing event name and version.
  - Compatibility mapping samples for moved/renamed event contracts.
  - Contract snapshots/baselines representing approved serialized schema.

- **Expected Outputs:**
  - Successful deserialization for compatible historical payloads.
  - Ignoring unknown properties.
  - Default values applied to missing optional fields.
  - Failure for breaking schema changes without version bump.
  - Correct type resolution from outbox metadata.
  - Stable JSON serialization output.

---

## 5. TDD Execution Plan

1. **RED**
   - Add a failing compatibility test for a schema evolution scenario:
     - missing field handling
     - unknown field tolerance
     - renamed property detection
     - moved type resolution
   - Confirm the current implementation fails where expected.

2. **GREEN**
   - Implement or adjust:
     - serializer settings
     - optional/default field handling
     - compatibility mapping in `IOutboxEventTypeResolver`
     - contract validation helpers
   - Re-run tests until all compatibility scenarios pass.

3. **REFACTOR**
   - Extract reusable fixtures/builders for:
     - historical payload generation
     - outbox metadata creation
     - contract snapshot assertions
   - Reduce duplication while preserving explicitness of evolution scenarios.

---

## 6. Exit Criteria

- [ ] Historical payloads deserialize successfully after additive schema changes.
- [ ] Unknown JSON properties are ignored safely.
- [ ] Missing optional fields are handled with default values.
- [ ] Breaking changes are detected when version is not incremented.
- [ ] Resolver supports historical event type compatibility mapping.
- [ ] Integration events remain immutable, data-only contracts.
- [ ] End-to-end historical outbox deserialization tests pass.
- [ ] CI includes schema compatibility validation.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── ArchitectureTests/
 │   └── Hector.ArchitectureTests/
 │       └── IntegrationEventSchemaArchitectureTests.cs
 │
 ├── UnitTests/
 │   └── Hector.Modules.[ModuleName].UnitTests/
 │       └── IntegrationEvents/
 │           ├── EventSchemaForwardCompatibilityTests.cs
 │           ├── EventSchemaBackwardCompatibilityTests.cs
 │           ├── EventSerializationContractSnapshotTests.cs
 │           └── OutboxEventTypeResolverCompatibilityTests.cs
 │
 └── IntegrationTests/
     └── Hector.Modules.[ModuleName].IntegrationTests/
     └── Outbox/
     └── HistoricalOutboxMessageCompatibilityTests.cs
```

## 8. Implementation Notes

- Snapshot/baseline verification should be used for serialized contract stability.
- Resolver tests should cover both:
  - canonical event identity resolution
  - compatibility mapping for legacy names/types
- Historical payload fixtures should be stored explicitly so schema evolution remains visible and reviewable.
- Tests should prefer production serializer settings to avoid false confidence from test-only configuration.

## Summary

This test plan ensures that [ADR-0031](/docs/adr/0031-event-schema-evolution-strategy.md) is enforced through automated validation of event schema evolution behavior. It protects the system against accidental breaking changes in serialized outbox messages and guarantees that historical events remain readable across codebase evolution, which is essential for replay, diagnostics, migration, and reliable event-driven operation.
