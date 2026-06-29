# Test Plan: ADR-0002 Initialize Project Structure for Modular DDD

## Status

Accepted

## Context

This test plan validates the structural integrity and dependency rules defined in [ADR-0002](/docs/adr/0002-initialize-project-structure.md). In a Modular DDD system, maintaining strict boundaries between **Building Blocks** and **Feature Modules**, and ensuring the correct **Dependency Direction** (inward towards Domain), is critical. Failure to enforce these rules leads to “**Big Ball of Mud**” architecture.

## Test Strategy

The validation strategy relies on **NetArchTest** to enforce architectural rules at the build/test level.

- **Architecture Guard Tests**: Enforce project layering, naming conventions, and dependency constraints.
- **Project Integrity Tests**: Ensure centralized configuration (Directory.Build.props) is applied.

**Target Project:** `tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

### Included

- Layering of `src/Framework` (Building Blocks).
- Standardized structure of `src/Modules/<FeatureName>`.
- Dependency directions (e.g., Domain must not depend on Infrastructure).
- Project naming conventions.
- Centralized package management verification.

### Excluded

- Internal logic of classes (covered by Unit Tests).
- Configuration values inside `appsettings.json`.

---

## 2. Test Cases (Architecture Guard Tests)

### TC-01: Should_NotHaveDependencyOnOtherLayers_When_ProjectIsDomainLayer

**Scenario:**

- Ensure the Domain project of any module remains pure and does not depend on `Application` or `Infrastructure`.

**Arrange:**

- Define all Domain projects using `NetArchTest.Rules`.

**Act:**

- Check for references to `Application` or `Infrastructure` assemblies.

**Assert:**

- Zero dependencies found.

---

### TC-02: Should_OnlyDependOnDomainAndBuildingBlocks_When_ProjectIsApplicationLayer

**Scenario:**

- Application layer should orchestrate but not know about persistence implementation details.

**Arrange:**

- Define all `Application` projects.

**Act:**

- Ensure they do not reference `Infrastructure` projects directly.

**Assert:**

- Dependencies are limited to Domain and `BuildingBlocks.Application/Domain`.

---

### TC-03: Should_ImplementModuleInterfacesAndDependOnCore_When_ProjectIsInfrastructureLayer

**Scenario:**

- Infrastructure should be the “outer” layer, depending on `Application` and Domain.

**Arrange:**

- Define all `Infrastructure` projects.

**Act:**

- Verify they reference the corresponding `Application` and Domain projects.

**Assert:**

- References exist to enable repository and service implementations.

---

### TC-04: Should_BePhysicallyIsolated_When_ComparingFeatureModules

**Scenario:**

- One module should not reference the internal projects of another module (e.g., `Catalog.Infrastructure` should not reference `Ordering.Domain`).

**Arrange:**

- Filter projects by module namespaces.

**Act:**

- Check for cross-module project references.

**Assert:**

- Only references to the `Contracts` project of other modules are allowed.

---

### TC-05: Should_UseCentralizedPackageManagement_When_DefiningDependencies

**Scenario:**

- Ensure all projects are governed by Directory.Packages.props.

**Arrange:**

- Scan project files (`.csproj`).

**Act:**

- Check for explicit `<Version>` tags in PackageReference.

**Assert:**

- No explicit versions found (versions must come from Central Package Management).

---

## 3. Non-Functional Validation Points

### 3.1 Build Performance

Ensure the project structure doesn’t lead to circular dependencies which break the build graph.

### 3.2 Consistency

Every module must follow the exact same 4-layer structure (`Domain`, `Application`, `Infrastructure`, `Contracts`).

---

## Test Data

- **Inputs:** The entire solution project graph (`Hector.slnx` or `.sln`).
- **Expected Outputs:** All layer-violation tests must pass.

---

## 5. TDD Execution Plan

1. **RED:** Write a test that fails if a `Domain` project references an `Infrastructure` project.
2. **GREEN:** Move repository implementations to `Infrastructure` and only keep interfaces in `Domain`.
3. **REFACTOR:** Move common structural rules to a base `ArchTest` class to avoid duplication for new modules.

---

## 6. Exit Criteria

- [ ] Architecture tests pass for all existing Building Blocks.
- [ ] Architecture tests pass for the `Projects` module (template module).
- [ ] Dependency graph tool (like `dotnet-dg`) confirms inward dependency flow.

---

## 7. Proposed Test File Layout

```text
tests/
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         ├── LayerDependencyTests.cs
         ├── ModuleStructureTests.cs
         └── CentralizedPackageTests.cs
```

---

## Summary

By automating the validation of [ADR-0002](/docs/adr/0002-initialize-project-structure.md), we ensure that the “**Modular**” part of our “**Modular Monolith**” remains a reality, not just a folder name. This prevents architectural erosion as the team and the codebase grow.

---
