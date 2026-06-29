# Test Plan: ADR-0012 Automated Persistence Mapping for Strongly Typed IDs

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR-0012](/docs/adr/0012-automated-persistence-mapping-for-strongly-typed-ids.md).

The project uses strongly typed identifiers to enforce type safety in the domain model. However, EF Core cannot natively map these types to primitive database columns such as `Guid`.

Without a centralized mechanism, each entity configuration would require manual `ValueConverter` registration, introducing repetitive boilerplate and increasing the risk of inconsistency across modules.

[ADR‑0012](/docs/adr/0012-automated-persistence-mapping-for-strongly-typed-ids.md) introduces an automated persistence mapping mechanism that:

- discovers strongly typed ID types across domain assemblies,
- registers EF Core `ValueConverters` centrally,
- converts strongly typed IDs to `Guid` for storage,
- reconstructs strongly typed IDs using reflection and a private `Guid` constructor during rehydration.
This test plan ensures that the automated persistence mapping mechanism works reliably, preserves domain purity, and eliminates configuration duplication across modules.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on converter behavior, reflection-based ID reconstruction, and assembly scanning logic.

  - **Target Project:** `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Focus on full EF Core persistence behavior, verifying that strongly typed IDs are stored as `Guid` and correctly rehydrated when entities are loaded from the database.

  - **Target Project:** `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

### Included

- Automatic discovery of strongly typed ID types from domain assemblies
- Registration of `StronglyTypedIdValueConverter<TId>`
- Conversion of strongly typed IDs to `Guid` for database storage
- Reconstruction of strongly typed IDs using private constructors
- Persistence and rehydration behavior inside EF Core
- Verification that domain models remain EF‑Core‑agnostic
- Validation of constructor contract required for persistence

### Excluded

- ID generation strategies (covered by other ADRs)
- Serialization or API formatting of IDs
- Non‑Guid strongly typed identifiers
- Primary constructor support (explicitly unsupported by [ADR‑0012](/docs/adr/0012-automated-persistence-mapping-for-strongly-typed-ids.md))

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_ConvertStronglyTypedIdToGuid_When_SavingEntity

**Scenario:**

- A strongly typed ID should be converted to a `Guid` when persisted to the database.

**Arrange:**

- Create a strongly typed identifier instance using its factory method.
- Configure the `StronglyTypedIdValueConverter<TId>`.

**Act:**

- Convert the identifier to the provider value.

**Assert:**

- The result is a `Guid`.
- The `Guid` value matches the underlying identifier value.

---

### TC-02

- #### Should_ReconstructStronglyTypedId_When_GuidIsReadFromDatabase

**Scenario:**

- When EF Core reads a Guid value from the database, the converter should reconstruct the strongly typed ID.

**Arrange:**

- Prepare a valid `Guid`.
- Configure the `StronglyTypedIdValueConverter<TId>`.

**Act:**

- Convert the provider value back to the strongly typed ID.

**Assert:**

- The resulting object is the expected strongly typed ID type.
- The identifier contains the original `Guid` value.

---

### TC-03

- #### Should_InvokePrivateGuidConstructor_When_RehydratingStronglyTypedId

**Scenario:**

- Rehydration should use reflection to invoke the non‑public constructor that accepts a `Guid`.

**Arrange:**

- Define a strongly typed ID with a private `Guid` constructor.

**Act:**

- Trigger conversion from `Guid` to strongly typed ID using the converter.

**Assert:**

- The private constructor is successfully invoked.
- The resulting instance contains the expected value.

---

### TC-04

- #### Should_PersistAndRehydrateStronglyTypedId_When_StronglyTypedIdConventionIsConfigured

**Scenario:**

- Entities containing strongly typed IDs should be persisted and rehydrated without manual configuration.

**Arrange:**

- Create an entity containing a strongly typed identifier.
- Use the configured EF Core DbContext.

**Act:**

- Persist the entity to the database.
- Retrieve the entity from the database.

**Assert:**

- The stored column value is a `Guid`.
- The rehydrated entity contains the correct strongly typed ID instance.
- The identifier value matches the original value.

---

### TC-05

- #### Should_RegisterValueConvertersAutomatically_When_StronglyTypedIdAssembliesAreConfigured

**Scenario:**

- The persistence layer should automatically register value converters for all discovered strongly typed ID types.

**Arrange:**

- Configure the persistence layer with domain assemblies containing ID types.

**Act:**

- Initialize the EF Core model.

**Assert:**

- Value converters are registered for all strongly typed ID properties.
- No manual converter configuration is required in entity mappings.

---

### TC-06

- #### Should_FailMaterialization_When_StronglyTypedIdDoesNotProvidePrivateGuidConstructor

**Scenario:**

- If a strongly typed ID does not provide the required private `Guid` constructor, EF Core rehydration should fail.

**Arrange:**

- Define a strongly typed ID type without the required constructor.

**Act:**

- Attempt to rehydrate the entity from the database.

**Assert:**

- EF Core materialization fails.
- The failure indicates that the ID cannot be reconstructed.

---

### TC-07

- #### Should_NotAllowPrimaryConstructors_ForStronglyTypedIdsUsedInPersistence

**Scenario:**

- Primary constructors are incompatible with the reflection-based rehydration mechanism.

**Arrange:**

- Define a strongly typed ID using a primary constructor.

**Act:**

- Attempt to rehydrate the ID through EF Core

**Assert:**

- Materialization fails.
- The failure indicates that the constructor cannot be resolved.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that internal reflection logic or persistence errors do not expose sensitive infrastructure details outside internal layers.

There must be no ambiguity in architecture tests or base abstractions.

### 3.2 Observability & Traceability

Verify that persistence operations involving strongly typed IDs are properly logged and do not break correlation or transaction tracing across the request pipeline.

### 3.3 Contract Stability

Verify that the persistence contract for strongly typed IDs remains stable and predictable across modules.

New identifiers should follow the required constructor contract to ensure persistence compatibility.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - `Guid.NewGuid()`
  - Valid strongly typed IDs such as `ProjectId`
  - Entities containing strongly typed identifiers

- **Expected Outputs:**
  - Strongly typed IDs stored as `Guid` values in the database
  - Rehydrated identifiers preserve original values
  - Materialization fails when constructor contract is violated

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:** Define failing tests verifying converter behavior and EF Core persistence compatibility.
2. **GREEN:**  Implement the automated mapping mechanism and value converter.
3. **REFACTOR:** Optimize reflection logic, reduce duplication, and ensure maintainability while keeping all tests passing.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Security/Non-functional points verified.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       └── Converters/
 │           └── StronglyTypedIdValueConverterTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── StronglyTypedIdMappingTests.cs
```

---

## Summary

This test plan validates that strongly typed identifiers can be automatically mapped to database columns without repetitive configuration. By centralizing EF Core value converter registration and enforcing a strict constructor contract, the architecture ensures consistent persistence behavior while keeping the domain model free from infrastructure concerns.

---
