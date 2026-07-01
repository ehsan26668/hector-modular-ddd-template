# Test Plan: ADR-0026 Define Event Serialization Strategy

## Status

Accepted

## Context

This test plan validates the Event Serialization Strategy introduced in [ADR‑0026](/docs/adr/0026-event-serialization-strategy.md).

The strategy defines how domain events are serialized into the `OutboxMessages` table using JSON with explicit contract metadata (`Type`, `Version`, `Content`). Since serialized events may live in the database for months or years, correctness, stability, and backward compatibility are critical.

This test plan ensures:

- Events can be reliably serialized and reconstructed.
- (`Name`, `Version`) uniquely identifies a contract.
- Logical event names are decoupled from CLR type names.
- Contract changes are detected early via architecture and snapshot tests.
- Backward compatibility rules are preserved.

This ADR is foundational for long‑term system evolution, cross-module communication, and refactoring safety.

## Test Strategy

- ### Unit Tests

  #### Focus on

  - `SystemTextJsonOutboxEventSerializer`
  - `AttributedOutboxEventTypeResolver`
  - Versioning and contract metadata validation

  (**Target Project**: tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests)

- ### Integration Tests

  #### Validate full round-trip

  - Domain event → OutboxMessage → Deserialization → CLR reconstruction

  (**Target Project**: tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests)

- ### Architecture Tests

  #### Enforce

  - Unique (`Name`, `Version`) combinations
  - Proper use of `OutboxEventAttribute`
  
  (**Target Project**: tests/ArchitectureTests/Hector.ArchitectureTests)

---

## 1. Scope

List exactly what is included and excluded from this test plan to set clear boundaries for the validation process.

### Included

- JSON serialization using `System.Text.Json`
- Storage of `Type`, `Version`, `Content`
- Resolution via `IOutboxEventTypeResolver`
- Contract identity enforcement (`Name`, `Version`)
- Duplicate contract prevention
- Snapshot protection of serialized contracts
- Backward-compatible deserialization scenarios

### Excluded

- Event upcasting logic
- Cross-service schema registry
- Alternative serialization formats
- Broker-level serialization concerns

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01

- #### Should_SerializeEvent_WithLogicalNameAndVersion

**Scenario:**

- Ensure that a domain event is serialized using its logical contract name and version, not its CLR type name.

**Arrange:**

- Define a domain event with `[OutboxEvent("projects.project-created", 1)]`.
- Instantiate event with sample data.

**Act:**

- Call `IOutboxEventSerializer.Serialize(event)`.

**Assert:**

- `Type == "projects.project-created"`
- `Version == 1`
- `Content` contains valid JSON
- CLR full type name does NOT appear in serialized metadata.

---

### TC-02

- #### Should_ResolveClrType_FromNameAndVersion

**Scenario:**

- Resolver must correctly map (`Name`, `Version`) to a CLR type.

**Arrange:**

- Register event type with attribute.
- Instantiate `AttributedOutboxEventTypeResolver`.

**Act:**

- Call `Resolve("projects.project-created", 1)`.

**Assert:**

- Returned type equals `typeof(ProjectCreatedDomainEvent)`.

---

### TC-03

- #### Should_ThrowException_When_UnknownContractRequested

**Scenario:**

- If an unknown (`Name`, `Version`) is requested, system must fail fast.

**Arrange:**

- Resolver without matching contract.

**Act:**

- Call `Resolve("unknown.event", 99)`.

**Assert:**

- Throws `InvalidOperationException` (or domain-specific exception).

---

### TC-04

- #### Should_EnforceUnique_NameAndVersionCombination

**Scenario:**

- Two event classes cannot share the same (Name, Version).

**Arrange:**

- Define two test events with identical attribute values.

**Act:**

- Execute architecture scan.

**Assert:**

- Architecture test fails.
- Error message clearly indicates duplicate contract.

---

### TC-05

- #### Should_Support_MultipleVersions_OfSameLogicalEvent

**Scenario:**

- Same logical name can exist with different versions.

**Arrange:**

- Define:
  - `[OutboxEvent("projects.project-created", 1)]`
  - `[OutboxEvent("projects.project-created", 2)]`

**Act:**

- Resolve both via resolver.

**Assert:**

- Version 1 resolves to V1 class.
- Version 2 resolves to V2 class.
- No conflict occurs.

---

### TC-06

- #### Should_ReconstructEvent_FromSerializedOutboxMessage

**Scenario:**

- Full round-trip validation.

**Arrange:**

- Create domain event instance.
- Serialize to OutboxMessage.
- Persist to in-memory test database.

**Act:**

- Load message.
- Resolve CLR type.
- Deserialize `Content`.

**Assert:**

- Reconstructed object equals original event.
- Property values match exactly.

---

### TC-07

- #### Should_PreserveBackwardCompatibility_When_NewPropertyAdded

**Scenario:**

- Additive schema evolution must not **break old events**.

**Arrange:**

- Version 1 event stored without new property.
- Version 2 class includes additional optional property.

**Act:**

- Deserialize old JSON into V1.
- Deserialize old JSON into V2 (if allowed).

**Assert:**

- Deserialization succeeds.
- Missing property defaults safely.

---

### TC-08

- #### Should_DetectContractChanges_UsingSnapshotTest

**Scenario:**

- Prevent accidental contract changes.

**Arrange:**

- Serialize event.
- Compare against stored snapshot (`event-contracts.snapshot`).

**Act:**

- Run snapshot test.

**Assert:**

- Test fails if JSON shape changes unexpectedly.
- Test passes when unchanged.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Ensure no internal CLR type names leak into persisted records.
- Verify that stack traces or internal metadata are not serialized.
- Confirm no sensitive internal infrastructure fields are exposed.

### 3.2 Observability & Traceability

- Ensure deserialization failures log:
  - `Type`
  - `Version`
  - Outbox message ID
- Ensure meaningful error messages for contract resolution failures.

### 3.3 Contract Stability

- Logical event names must remain constant.
- Renaming CLR classes must not affect stored records.
- Snapshot tests must fail on breaking property renames or removals.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Example Domain Event:

    ```json
    {
      "ProjectId": "6e0b8a6e-45a8-4d64-92e0-8c0d44d8a55a",
      "OccurredOn": "2026-05-12T10:23:44Z"
    }
    ```

  - Metadata:

    ```text
    Type: projects.project-created
    Version: 1
    ```

- **Expected Outputs:**
  - Successful resolution to correct CLR type.
  - Accurate reconstruction.
  - Stable JSON format.

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   - Write failing test for resolving logical name to CLR type.
   - Write failing test for duplicate (`Name`, `Version`) detection.
   - Write failing round-trip reconstruction test.

2. **GREEN:**  
   - Implement `SystemTextJsonOutboxEventSerializer`.
   - Implement `AttributedOutboxEventTypeResolver`.
   - Add architecture validation for uniqueness.

3. **REFACTOR:**  
   - Introduce caching for resolver lookups.
   - mprove error messages.
   - Optimize serializer configuration.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Architecture tests enforce uniqueness.
- [ ] Snapshot tests detect contract changes.
- [ ] No CLR type names persisted in outbox records.
- [ ] Round-trip reconstruction validated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       ├── OutboxEventSerializationTests.cs
 │       ├── OutboxEventTypeResolverTests.cs
 │       ├── OutboxEventVersioningTests.cs
 │       └── OutboxEventSerializationCompatibilityTests.cs
 ├── IntegrationTests/
 │   └── Hector.BuildingBlocks.Persistence.IntegrationTests/
 │       └── OutboxProcessorDeserializationTests.cs
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         ├── EventContractArchitectureTests.cs
         └── EventContractSnapshotTests.cs
```

---

## Summary

[ADR‑0026](/docs/adr/0026-event-serialization-strategy.md) establishes a robust and future‑proof event serialization strategy.

This test plan guarantees:

- Stable contract identity
- Safe long-term storage
- Version-aware reconstruction
- Protection against accidental breaking changes

With this validated, your Outbox is now evolution-ready — which is exactly what a long‑lived modular DDD system demands.

---
