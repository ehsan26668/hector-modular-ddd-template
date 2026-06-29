# Test Plan: ADR-0019 Simplify StronglyTypedId and Use Assembly Scanning

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR‑0019](/docs/adr/0019-simplify-strongly-typed-id-and-use-assembly-scanning.md): *Simplify StronglyTypedId and Use Assembly Scanning*.

The decision standardizes how strongly typed identifier assemblies are exposed and discovered across feature modules. Instead of manual registration or per-type configuration, the persistence layer automatically discovers identifier types through assembly scanning and registers the appropriate EF Core value converters.

A composite provider (`CompositeStronglyTypedIdAssemblyProvider`) aggregates module-level providers while avoiding circular dependencies in the dependency injection container.

This test plan ensures that:

- module-level strongly typed ID providers are discovered correctly
- the composite provider aggregates providers without creating circular dependencies
- EF Core value converters are applied automatically
- persistence configuration remains modular and extensible

These guarantees are critical for maintaining modular boundaries and reducing manual persistence configuration as new modules are introduced.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Focus on provider discovery, composite provider behavior, and registration logic.

  - **Target Project:** `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`

- **Integration Tests:**
  - Validate EF Core model configuration and automatic strongly typed ID converter registration in real DbContext usage.

  - **Target Project:** `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

---

## 1. Scope

List exactly what is included and excluded from this test plan to set clear boundaries for the validation process.

### Included

- Discovery of `IStronglyTypedIdAssemblyProvider` implementations through assembly scanning
- Composition of multiple providers through `CompositeStronglyTypedIdAssemblyProvider`
- Correct dependency injection registration pattern
- Automatic application of `StronglyTypedIdValueConverter<TId>` in EF Core
- Successful resolution of providers without circular dependency

### Excluded

- Business logic inside modules
- Domain identity generation rules (covered by [ADR‑0018](/docs/adr/0018-domain-identity-generation-policy.md))
- Database-specific behavior beyond EF Core mapping
- Integration event processing

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_DiscoverAssemblyProviders_When_ScanningAssemblies

**Scenario:**

- Assembly scanning must correctly detect all implementations of `IStronglyTypedIdAssemblyProvider`.

**Arrange:**

- Prepare a set of assemblies containing provider implementations.
- Ensure providers implement `IStronglyTypedIdAssemblyProvider`.

**Act:**

- Execute the assembly scanning logic.

**Assert:**

- All valid provider types are discovered.
- Abstract classes and interfaces are excluded.
- `CompositeStronglyTypedIdAssemblyProvider` is excluded from discovery.

---

### TC-02

- #### Should_CreateCompositeProvider_When_MultipleProvidersExist

**Scenario:**

- The composite provider should aggregate all concrete providers discovered during registration.

**Arrange:**

- Register multiple concrete provider types in the service collection.

**Act:**

- Resolve `IStronglyTypedIdAssemblyProvider` from the DI container.

**Assert:**

- The resolved instance is `CompositeStronglyTypedIdAssemblyProvider`.
- The composite contains all discovered providers.

---

### TC-03

- #### Should_NotCreateCircularDependency_When_ResolvingCompositeProvider

**Scenario:**

- The DI container must resolve the composite provider without creating circular dependencies.

**Arrange:**

- Configure services using the registration strategy defined in [ADR‑0019](/docs/adr/0019-simplify-strongly-typed-id-and-use-assembly-scanning.md).

**Act:**

- Build the service provider and resolve `IStronglyTypedIdAssemblyProvider`.

**Assert:**

- Resolution succeeds without exception.
- No circular dependency occurs.

---

### TC-04

- #### Should_ApplyValueConverters_When_DbContextModelIsBuilt

**Scenario:**

- EF Core must automatically apply `StronglyTypedIdValueConverter<TId>` to entity identifier properties.

**Arrange:**

- Create a test DbContext containing entities that use strongly typed IDs.

**Act:**

- Trigger `OnModelCreating`.

**Assert:**

- Strongly typed ID properties have the correct value converter.
- The underlying database type maps to `Guid`.

---

### TC-05

- #### Should_DiscoverModuleIdentifierAssemblies_When_ModuleRegistersProvider

**Scenario:**

- Feature modules must expose their identifier assemblies through provider implementations.

**Arrange:**

- Create a module-level provider (e.g., `ProjectsStronglyTypedIdAssemblyProvider`).

**Act:**

- Run the scanning and provider composition process.

**Assert:**

- The module assembly is included in the composite provider.
- Identifier types from that module are discoverable.

---

### TC-06

- #### Should_RegisterConvertersForAllStronglyTypedIds_When_MultipleModulesExist

**Scenario:**

- When multiple modules define strongly typed IDs, converters must be registered for all identifier types.

**Arrange:**

- Provide multiple module assemblies containing strongly typed IDs.

**Act:**

- Initialize the DbContext model.

**Assert:**

- Converters are registered for all discovered identifier types.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Ensure assembly scanning does not expose internal system details such as file paths or internal infrastructure types in logs or public APIs.

### 3.2 Observability & Traceability

Verify that startup logging can identify discovered providers and assemblies to assist with debugging module registration issues.

### 3.3 Contract Stability

Ensure the contract `IStronglyTypedIdAssemblyProvider` remains stable and compatible with future modules.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Module assemblies
  - Provider implementations
  - Entities using strongly typed IDs
  - EF Core DbContext initialization

- **Expected Outputs:**
  - Discovered provider list
  - Composite provider instance
  - EF Core value converters applied to strongly typed ID properties

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Write failing tests verifying assembly scanning and composite provider resolution.

2. **GREEN:**  
   Implement provider discovery and composite provider registration.

3. **REFACTOR:**  
   Simplify scanning logic and ensure maintainability for future modules.

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
 │       └── StronglyTypedIds/
 │           ├── StronglyTypedIdAssemblyProviderDiscoveryTests.cs
 │           ├── CompositeStronglyTypedIdAssemblyProviderTests.cs
 │           └── StronglyTypedIdRegistrationTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Persistence.IntegrationTests/
         └── StronglyTypedIdConverterIntegrationTests.cs
```

---

## Summary

This test plan ensures that strongly typed identifier registration remains modular, automatic, and safe from dependency injection cycles. By validating assembly discovery, composite provider behavior, and EF Core converter registration, the system guarantees consistent persistence mapping for strongly typed identifiers across all modules.

---
