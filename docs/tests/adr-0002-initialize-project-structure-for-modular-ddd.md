# Test Plan: ADR-0002 Initialize Project Structure for Modular DDD

## Status

Accepted

## Context

This test plan validates the structural integrity and dependency rules defined in [ADR-0002](/docs/adr/0002-initialize-project-structure.md). In a Modular DDD system, maintaining strict boundaries between Framework building blocks, Feature Modules, Host composition, and test categories is critical to prevent architectural erosion and preserve long-term maintainability.

## Test Strategy

The validation strategy relies primarily on architecture and structural tests to enforce the solution shape and dependency rules defined by ADR-0002.

- **Architecture Guard Tests**: Enforce project layering, dependency direction, and module isolation.
- **Structure Validation Tests**: Enforce the expected solution folders, projects, and standardized module/test layout.
- **Project Integrity Tests**: Ensure centralized build and package management configuration exists and is respected.

**Target Project:** `tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

### Included

- Expected project structure under `src/Framework`
- Expected unified host location under `src/Host/Hector.Host`
- Standardized structure of `src/Modules/<FeatureName>`
- Dependency directions between Domain, Application, and Infrastructure layers
- Module isolation rules across feature modules
- Centralized build and package management configuration
- Standardized test category structure under `tests/`

### Excluded

- Internal class logic and business behavior
- Dependency injection wiring correctness
- Middleware runtime behavior
- Configuration values inside `appsettings.json`
- Template generation behavior

---

## 2. Test Cases

### TC-01: Should_NotDependOnApplicationOrInfrastructure_When_ProjectIsDomainLayer

**Scenario:**

Ensure the Domain project of any module remains pure and does not depend on `Application` or `Infrastructure`.

**Arrange:**

Identify all module Domain assemblies.

**Act:**

Inspect dependencies for references to module Application or Infrastructure assemblies.

**Assert:**

No such dependencies exist.

---

### TC-02: Should_NotDependOnInfrastructure_When_ProjectIsApplicationLayer

**Scenario:**

Application layer should orchestrate use cases without depending directly on Infrastructure implementation details.

**Arrange:**

Identify all module Application assemblies.

**Act:**

Inspect dependencies for references to Infrastructure assemblies.

**Assert:**

No Application assembly directly depends on Infrastructure.

---

### TC-03: Should_DependOnApplicationAndDomain_When_ProjectIsInfrastructureLayer

**Scenario:**

Infrastructure acts as the outer layer and must integrate with the module’s Domain and Application layers.

**Arrange:**

Identify all module Infrastructure assemblies.

**Act:**

Inspect references to corresponding Domain and Application assemblies.

**Assert:**

Required references to Domain and Application exist.

---

### TC-04: Should_OnlyAllowCrossModuleCommunicationThroughContracts_When_ComparingFeatureModules

**Scenario:**

A feature module must not depend on another module’s internal layers.

**Arrange:**

Identify all module assemblies and group them by module.

**Act:**

Inspect cross-module dependencies.

**Assert:**

Dependencies on other modules are forbidden except through that module’s `Contracts` assembly.

---

### TC-05: Should_UseCentralPackageManagement_When_DefiningPackageReferences

**Scenario:**

All package versions must be centrally managed through `Directory.Packages.props`.

**Arrange:**

Inspect all project files in the solution.

**Act:**

Check whether any `PackageReference` declares an explicit `Version`.

**Assert:**

No project file declares package versions inline.

---

### TC-06: Should_ContainStandardLayers_When_ModuleExists

**Scenario:**

Each module under `src/Modules/` must follow the standard 4-layer structure.

**Arrange:**

Enumerate all module directories.

**Act:**

Verify the presence of `Domain`, `Application`, `Infrastructure`, and `Contracts` folders and their corresponding `.csproj` files.

**Assert:**

Every module contains all four layers with correctly named project files.

---

### TC-07: Should_ContainUnifiedHostProject_When_SolutionStructureIsValidated

**Scenario:**

The solution must contain a unified host project located at `src/Host/Hector.Host`.

**Arrange:**

Inspect the `src/Host` directory.

**Act:**

Check for the existence of the `Hector.Host` project.

**Assert:**

`src/Host/Hector.Host/Hector.Host.csproj` exists.

---

### TC-08: Should_ContainExpectedFrameworkProjects_When_SolutionStructureIsValidated

**Scenario:**

The Framework area must contain the expected building block and architecture testing projects defined by ADR-0002.

**Arrange:**

Inspect the `src/Framework` directory.

**Act:**

Check for the existence of the expected projects.

**Assert:**

The following projects exist:

- `Hector.BuildingBlocks.Domain`
- `Hector.BuildingBlocks.Application`
- `Hector.BuildingBlocks.Persistence`
- `Hector.BuildingBlocks.Web`
- `Hector.ArchitectureTests.Framework`

---

### TC-09: Should_ContainStandardTestCategories_When_TestStructureIsValidated

**Scenario:**

The solution must organize tests into standardized categories.

**Arrange:**

Inspect the `tests/` directory.

**Act:**

Check for the presence of standard test category folders.

**Assert:**

The following folders exist:

- `ArchitectureTests`
- `UnitTests`
- `IntegrationTests`
- `TemplateTests`
- `Shared`

---

### TC-10: Should_ContainCentralizedBuildFiles_When_SolutionStructureIsValidated

**Scenario:**

The solution must use centralized build and dependency configuration.

**Arrange:**

Inspect the solution root.

**Act:**

Check for the required root configuration files.

**Assert:**

The following files exist:

- `Directory.Build.props`
- `Directory.Packages.props`

---

### TC-11: Should_FollowProjectNamingAndPlacementConventions_When_SolutionStructureIsValidated

**Scenario:**

Projects should follow the naming and placement conventions defined by the Modular DDD structure.

**Arrange:**

Inspect Framework, Module, Host, and test project locations.

**Act:**

Validate project names against their folder placement.

**Assert:**

- Framework projects follow `Hector.BuildingBlocks.*` or `Hector.ArchitectureTests.Framework`
- Module projects follow `Hector.Modules.<Feature>.<Layer>`
- Host project follows `Hector.Host`
- Test projects follow the established `Hector.*.Tests` naming pattern where applicable

---

## 3. Non-Functional Validation Points

### 3.1 Build Graph Integrity

Ensure the project structure does not introduce circular dependencies that break the solution build graph.

### 3.2 Structural Consistency

Every module must follow the same 4-layer structure and naming scheme.

### 3.3 Architectural Maintainability

Structural rules must be enforceable through automated tests so that future modules follow the same conventions.

---

## 4. Test Data

- **Inputs:** Solution structure, project files, and assembly dependency graph
- **Expected Outputs:** All structural and architectural rules pass for the current solution layout

---

## 5. TDD Execution Plan

1. **RED:** Write failing structure and dependency tests for missing layers or invalid references.
2. **GREEN:** Adjust project placement, dependencies, or naming until the tests pass.
3. **REFACTOR:** Extract reusable structure and dependency assertions to shared architecture testing helpers.

---

## 6. Exit Criteria

- [x] Architecture tests pass for Framework and module dependency rules
- [x] Architecture tests pass for the `Projects` module structure
- [x] Unified host structure is validated
- [x] Standard test category structure is validated
- [x] Centralized build and package management structure is validated

---

## 7. Proposed Test File Layout

```text
tests/
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         ├── LayerBoundaries/
         │   ├── LayerDependencyTests.cs
         │   └── ModuleIsolationTests.cs
         ├── Structure/
         │   ├── ModuleStructureTests.cs
         │   ├── FrameworkStructureTests.cs
         │   ├── HostStructureTests.cs
         │   ├── TestStructureTests.cs
         │   └── CentralizedBuildTests.cs
         └── Conventions/
             └── ProjectNamingConventionTests.cs
```

## Summary

By automating the validation of [ADR-0002](/docs/adr/0002-initialize-project-structure.md), the solution continuously enforces its intended Modular DDD structure across Framework, Modules, Host, and Tests. This protects the architecture from structural drift as the codebase evolves.
