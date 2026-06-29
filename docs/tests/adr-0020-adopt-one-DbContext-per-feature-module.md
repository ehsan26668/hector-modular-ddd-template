# Test Plan: ADR-0020 Adopt One DbContext per Feature Module

## Status

Accepted

## Context

This test plan validates the architectural decision defined in [ADR‑0020](/docs/adr/0020-adopt-one-dbcontext-per-feature-module.md): *Adopt One DbContext per Feature Module*.

The decision mandates that each feature module owns its own EF Core DbContext located inside the module’s Infrastructure layer. This DbContext manages only its module-specific domain entities and uses a dedicated database schema. Modules may not directly use each other’s DbContexts.

The test plan ensures that:

- each module defines a separate DbContext
- DbContexts do not include cross-module entity sets
- module boundaries are preserved at the persistence level
- BaseDbContext conventions (e.g., domain event dispatching, strongly typed ID mapping) remain consistently applied
- no shared or global DbContext is used anywhere in the system

This validation is critical for maintaining independent module persistence models, enabling modular migrations, and supporting potential future extraction of feature modules into microservices.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Validate structure, conventions, DbContext inheritance, and schema configuration.

  - **Target Project:** `Hector.Modules.[ModuleName].Infrastructure.UnitTests`

- **Integration Tests:**
  - Validate EF Core behavior, migrations, domain event dispatching within the module-specific DbContext, and independence between module DbContexts.

  - **Target Project:** `Hector.Modules.[ModuleName].IntegrationTests`

---

## 1. Scope

List exactly what is included and excluded from this test plan to set clear boundaries for the validation process.

### Included

- Verification that every module defines its own DbContext
- Validation that DbContexts inherit from `BaseDbContext`
- Validation that DbContexts only expose entities from their own module
- Verification that modules do not reference each other’s DbContexts
- Schema naming conventions (module-specific schema)
- Isolation of persistence configuration per module

### Excluded

- Testing the internal behavior of BaseDbContext (covered elsewhere)
- Business logic inside modules
- Specific entity-level validation or domain rules
- Query performance or database-level optimization

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_DefineDbContextInsideInfrastructureLayer_When_ModuleIsCreated

**Scenario:**

- Each module must define a DbContext inside its Infrastructure project.

**Arrange:**

- Load module assemblies.
- Locate Infrastructure-layer types.

**Act:**

- Search for types inheriting from `BaseDbContext`.

**Assert:**

- Exactly one DbContext exists per module.
- The DbContext class resides inside the module’s Infrastructure project.

---

### TC-02

- #### Should_InheritFromBaseDbContext_When_ModuleDefinesDbContext

**Scenario:**

- All module DbContexts must share common conventions through `BaseDbContext`.

**Arrange:**

- Inspect module DbContext type.

**Act:**

- Verify inheritance.

**Assert:**

- DbContext derives from `BaseDbContext`.
- No DbContext bypasses the shared base class.

---

### TC-03

- #### Should_ExposeOnlyModuleEntities_When_DefiningDbSets

**Scenario:**

- DbContext must only include domain entities belonging to its module.

**Arrange:**

- Discover entity types referenced by `DbSet<T>` properties.

**Act:**

- Compare entity namespaces with the module’s domain namespace.

**Assert:**

- No DbSet references an entity from another feature module.

---

### TC-04

- #### Should_NotReferenceOtherModuleDbContexts_When_ConfiguringPersistence

**Scenario:**

- Cross-module DbContext injection is forbidden.

**Arrange:**

- Inspect DI registration and constructor injection across modules.

**Act:**

- Use reflection to detect any cross-module DbContext dependencies.

**Assert:**

- No module depends on another module’s DbContext.

---

### TC-05

- #### Should_ApplyModuleSpecificSchema_When_ModelIsConfigured

**Scenario:**

- Each module DbContext must configure its database schema (e.g., `projects.Projects`).

**Arrange:**

- Create an instance of the module DbContext.

**Act:**

- Inspect EF Core model metadata (schema names).

**Assert:**

- All entities for the module use the correct module schema.

---

### TC-06

- #### Should_AllowIndependentMigrations_When_GeneratingEfCoreMigrations

**Scenario:**

- Each module must generate schema changes independently.

**Arrange:**

- Create a test migration for a module.

**Act:**

- Run migration generation.

**Assert:**

- Migration includes only entities from that module.
- No entities from other modules appear.

---

### TC-07

- #### Should_DispatchDomainEvents_When_SavingChangesInModuleDbContext

**Scenario:**

- Domain event dispatching must work per module via BaseDbContext.

**Arrange:**

- Create an aggregate that raises a domain event inside a module.
- Insert it via the module’s DbContext.

**Act:**

- Call `SaveChangesAsync`.

**Assert:**

- Domain events were dispatched.
- Only module-specific handlers are invoked.

---

### TC-08

- #### Should_NotInterfereWithOtherModules_When_UsingSeparateDbContexts

**Scenario:**

- Saving changes in one module must not affect another module’s persistence.

**Arrange:**

- Instantiate two module DbContexts (e.g., `ProjectsDbContext` and `AnotherModuleDbContext`).

**Act:**

- Perform updates independently.

**Assert:**

- No cross-module contamination occurs.
- Transaction boundaries are separate (unless explicitly using Outbox patterns).

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Ensure module DbContexts do not leak table names, schemas, or internal persistence details into API-level responses.

### 3.2 Observability & Traceability

- Ensure logs clearly indicate which module DbContext performed a given operation.
- Correlation IDs must propagate through module-specific persistence layers.

### 3.3 Contract Stability

- Ensure DbContext-related abstractions and DI patterns remain stable so additional modules can be added without breaking existing modules.

---

## 4. Test Data

Define specific sample data, edge cases, or sanitized examples used during testing:

- **Inputs:**
  - Module assemblies
  - Test domain entities
  - Multiple DbContext instances

- **Expected Outputs:**
  - Isolated DbContexts
  - Correct module schemas
  - No cross-module entity or DbContext leaks

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Write failing tests that assume cross-module DbContext or missing schemas.

2. **GREEN:**  
   Implement module DbContexts and registration patterns.

3. **REFACTOR:**  
   Consolidate DbContext configuration and ensure strong modular boundaries.

---

## 6. Exit Criteria

List the conditions that must be met for this ADR to be considered successfully validated:

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Schema isolation is confirmed.
- [ ] No cross-module DbContext usage found.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Modules.[ModuleName].Infrastructure.UnitTests/
 │       └── DbContext/
 │           ├── ModuleDbContextStructureTests.cs
 │           ├── ModuleDbContextSchemaTests.cs
 │           └── ModuleDbContextIsolationTests.cs
 └── IntegrationTests/
     └── Hector.Modules.[ModuleName].IntegrationTests/
         └── Persistence/
             └── ModuleDbContextPersistenceTests.cs
```

---

## Summary

This test plan confirms that each feature module owns its own persistence boundary using an isolated DbContext. The tests ensure that EF Core configuration is modular, schemas remain isolated, cross-module contamination is prevented, and BaseDbContext conventions are consistently applied. This strengthens the modular monolith structure and positions the system for easier scaling and potential module extraction.

---
