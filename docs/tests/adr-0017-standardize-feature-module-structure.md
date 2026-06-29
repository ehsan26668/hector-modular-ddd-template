# Test Plan: ADR-0017 Standardize Feature Module Structure

## Status

Accepted

## Context

This test plan validates the architectural decision defined in

[ADR‑0017](/docs/adr/0017-standardize-feature-module-structure.md): *Standardize Feature Module Structure*.

The decision introduces a standardized structure for feature modules in the Hector modular monolith architecture. Each module must follow an internal Clean Architecture layout consisting of:

- Domain
- Application
- Infrastructure
- Contracts

The primary goal of this structure is to enforce strict module boundaries and maintain clear dependency direction between layers. This ensures that modules remain independently understandable, testable, and evolvable.

This test plan verifies that:

- modules follow the standardized project layout
- layer dependencies respect the defined architecture rules
- modules do not depend on implementation details of other modules
- cross-module communication occurs only through contracts or events

These guarantees are essential for preserving the modular monolith architecture and preventing architectural drift as the project grows.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Validate architecture rules using reflection and NetArchTest to enforce dependency direction and module boundaries.

  - **Target Project:** `tests/ArchitectureTests/Hector.ArchitectureTests`

- **Integration Tests:**
  - Validate that modules can be composed by the host application and operate correctly through dependency injection and mediator pipelines.

  - **Target Project:** `Hector.Modules.Projects.IntegrationTests`

---

## 1. Scope

### Included

- Validation of the standardized module project structure
- Validation of dependency direction between module layers
- Prevention of cross-module implementation dependencies
- Verification that modules expose contracts for external communication
- Host application module composition

### Excluded

- Business logic of specific modules
- API endpoint implementation details
- Domain model correctness
- Infrastructure implementation details such as persistence mapping

---

## 2. Test Cases (Unit / Integration)

### TC-01

- #### Should_FollowStandardModuleProjectStructure_When_ModuleIsCreated

**Scenario:**

- A feature module must contain the standardized project layout defined in [ADR‑0017](/docs/adr/0017-standardize-feature-module-structure.md).

**Arrange:**

- Load the assembly structure for a module (e.g., Projects module).

**Act:**

- Inspect the module directory and project names.

**Assert:**

- The module contains the following projects:
  - `<Module>.Domain`
  - `<Module>.Application`
  - `<Module>.Infrastructure`
  - `<Module>.Contracts`

---

### TC-02

- #### Should_DispatchAllCollectedDomainEvents_When_MultipleAggregatesContainEvents

**Scenario:**

- The Domain layer must not depend on Application or Infrastructure layers.

**Arrange:**

- Load the Domain assembly of a module.

**Act:**

- Analyze assembly dependencies using NetArchTest.

**Assert:**

- Domain does not reference:
  - Application
  - Infrastructure
  - other module implementations

---

### TC-03

- #### Should_AllowApplicationToDependOnDomainAndContracts_When_ValidatingDependencies

**Scenario:**

- Application layer must depend only on Domain and Contracts.

**Arrange:**

- Load the Application assembly.

**Act:**

- Analyze dependencies.

**Assert:**

- Application references Domain and Contracts only.
- Application does not reference Infrastructure.

---

### TC-04

- #### Should_AllowInfrastructureToDependOnApplicationAndDomain_When_ValidatingDependencies

**Scenario:**

- Infrastructure implements persistence and integration details and may depend on Application and Domain.

**Arrange:**

- Load Infrastructure assembly.

**Act:**

- Analyze dependencies.

**Assert:**

- Infrastructure references Application and Domain.
- Infrastructure does not leak dependencies into other modules’ Infrastructure layers.

---

### TC-05

- #### Should_ExposePublicContractsThroughContractsProject_When_ModuleCommunicatesExternally

**Scenario:**

- Cross-module communication must occur through Contracts.

**Arrange:**

- Inspect Contracts assembly.

**Act:**

- Search for integration events or shared contract types.

**Assert:**

- Public contract types exist only in Contracts assemblies.
- Other modules reference Contracts instead of internal implementations.

---

### TC-06

- #### Should_ComposeModulesThroughHost_When_ApplicationStarts

**Scenario:**

- Modules must be composed through the host application during startup.

**Arrange:**

- Start the application using the test host.
- Register modules through dependency injection.

**Act:**

- Resolve module services.

**Assert:**

- Module services are successfully resolved.
- Modules integrate correctly with mediator and persistence layers.

---

### TC-07

- #### Should_PreventCrossModuleImplementationDependency_When_ValidatingArchitectureRules

**Scenario:**

- A module must not directly depend on another module’s Application, Domain, or Infrastructure assemblies.

**Arrange:**

- Load module assemblies.

**Act:**

- Analyze references.

**Assert:**

- Cross-module references exist only through Contracts.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Verify that module boundaries prevent internal implementation details from leaking outside module assemblies.

### 3.2 Observability & Traceability

Ensure module interactions preserve correlation and tracing context through mediator pipelines and messaging components.

### 3.3 Contract Stability

Verify that public module contracts are stable and isolated from internal implementation changes.

---

## 4. Test Data

Define specific sample data used during testing:

- **Inputs:**
  - Module assemblies
  - Dependency graphs
  - DI container registrations
  - Integration event contracts

- **Expected Outputs:**
  - Valid module structure
  - Correct dependency direction
  - Successful module composition in the host
  - No illegal cross-module dependencies

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED:**  
   Write architecture tests verifying module structure and dependency direction.

2. **GREEN:**  
   Implement module projects following the standardized structure.

3. **REFACTOR:**  
   Refine module boundaries and ensure contracts are correctly exposed.

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
 ├── ArchitectureTests/
 │   └── Hector.ArchitectureTests/
 │       └── Modules/
 │           ├── ModuleStructureTests.cs
 │           └── ModuleDependencyRulesTests.cs
 └── IntegrationTests/
     └── Hector.Modules.Projects.IntegrationTests/
         └── ModuleCompositionTests.cs
```

---

## Summary

This test plan validates the standardized feature module structure introduced in [ADR‑0017](/docs/adr/0017-standardize-feature-module-structure.md). The tests ensure that all modules follow the defined Clean Architecture layout, maintain strict dependency direction, and interact through explicit contracts. By enforcing these architectural rules through automated tests, the system prevents architectural drift and preserves the modular monolith boundaries as the platform evolves.

---
