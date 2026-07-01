# Test Plan: ADR-0037 Introduce ModuleLoader for Automatic Module Registration

## Status

Accepted

## Context

This test plan validates the **ModuleLoader automatic module registration mechanism** defined in [ADR-0037](/docs/adr/0037-introduce-module-loader-for-auto-registration.md).

The system follows a **Modular Monolith architecture** where the application is divided into feature modules. Each module encapsulates:

- Domain
- Application
- Infrastructure
- Contracts

Previous architectural decisions established several conventions:

- ADR‑0017 — Standardize Feature Module Structure
- ADR‑0019 — Simplify StronglyTypedId and Use Assembly Scanning
- ADR‑0020 — One DbContext per Feature Module

Despite these conventions, module registration in the Host currently requires **manual configuration inside Program.cs**, including:

- Application services
- Mediator handlers
- Infrastructure services
- DbContext
- StronglyTypedId assembly providers

This approach introduces several architectural problems:

- Boilerplate in the composition root
- Risk of forgetting module registrations
- Tight coupling between Host and feature modules
- Reduced scalability as modules grow

ADR‑0037 introduces a **ModuleLoader infrastructure** responsible for:

- Discovering modules automatically
- Registering them during application startup
- Keeping the Host independent from feature modules

Each module exposes a contract via:

- IModuleIdentity

The ModuleLoader discovers implementations of this interface and executes their `Register` method automatically.
This test plan ensures that the ModuleLoader works reliably and enforces the intended architectural constraints.

---

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests**
  - Validate module discovery logic.
  - Validate correct invocation of module registration.
  - Target Project:  
`tests/UnitTests/Hector.BuildingBlocks.Application.UnitTests`

- **Integration Tests**
  - Validate end-to-end module registration during application startup.
  - Ensure modules are correctly wired without manual registration.
  - Target Project:  
`tests/IntegrationTests`

- **Architecture Tests**
  - Validate architectural rules ensuring modules expose `IModuleIdentity`.
  - Validate Host independence from modules.
  - Target Project:  
`tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

### Included

- Automatic discovery of `IModuleIdentity` implementations
- Execution of module registration logic
- Registration of application and infrastructure services
- Module isolation from Host
- Assembly scanning behavior
- Correct invocation order during startup
- Architectural rule enforcing module identity contract

### Excluded

- Runtime module unloading
- Dynamic plugin loading
- Cross-process module discovery
- Advanced module metadata management

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_DiscoverModules_When_ScanningAssemblies

**Scenario:**  
The ModuleLoader must discover all implementations of `IModuleIdentity`.

**Arrange:**

- Load assemblies containing module implementations

**Act:**

- Execute ModuleLoader assembly scan

**Assert:**

- All `IModuleIdentity` implementations are discovered
- No unrelated types are returned

---

### TC-02: Should_InvokeModuleRegisterMethod_When_ModuleDiscovered

**Scenario:**  
Discovered modules must have their `Register` method executed.

**Arrange:**

- Create a test module implementing `IModuleIdentity`
- Register ModuleLoader

**Act:**

- Execute module registration

**Assert:**

- `Register` method is called
- Services defined in module are registered

---

### TC-03: Should_RegisterModuleServices_When_ModuleLoaderExecutes

**Scenario:**  
Module services must be registered automatically.

**Arrange:**

- Configure a test service inside module registration

**Act:**

- Execute `AddModules()` extension method

**Assert:**

- Service is available in the service provider

---

### TC-04: Should_NotRequireManualRegistration_When_ModuleLoaderIsUsed

**Scenario:**  
Modules should not require manual registration inside `Program.cs`.

**Arrange:**

- Configure Host without explicit module registration

**Act:**

- Build application

**Assert:**

- Module services are still registered
- Application starts successfully

---

### TC-05: Should_HandleMultipleModules_When_DiscoveredAutomatically

**Scenario:**  
ModuleLoader must support multiple modules.

**Arrange:**

- Provide multiple modules implementing `IModuleIdentity`

**Act:**

- Execute module discovery

**Assert:**

- All modules are registered
- Each module's `Register` method is executed

---

### TC-06: Should_NotRegisterDuplicateModules_When_ScanningAssemblies

**Scenario:**  
Duplicate module registrations must be prevented.

**Arrange:**

- Provide duplicate module implementations

**Act:**

- Execute module discovery

**Assert:**

- Modules are registered only once

---

### TC-07: Should_KeepHostIndependentFromModules_When_UsingModuleLoader

**Scenario:**  
Host must not directly reference feature modules.

**Arrange:**

- Inspect Host assembly dependencies

**Act:**

- Analyze referenced assemblies

**Assert:**

- Host does not reference module assemblies directly

---

### TC-08: Should_OnlyRegisterTypesImplementingIModuleIdentity_When_ScanningAssemblies

**Scenario:**  
Only valid module identity implementations must be discovered.

**Arrange:**

- Include unrelated types in assemblies

**Act:**

- Execute ModuleLoader scan

**Assert:**

- Only `IModuleIdentity` implementations are returned

---

### TC-09: Should_AllowModulesToRegisterInfrastructureAndApplicationServices

**Scenario:**  
Modules must register both application and infrastructure dependencies.

**Arrange:**

- Configure module registration with multiple services

**Act:**

- Execute module registration

**Assert:**

- All module services exist in the service provider

---

### TC-10: Should_ThrowClearException_When_ModuleRegistrationFails

**Scenario:**  
Module registration failures must be detectable.

**Arrange:**

- Create module with failing registration logic

**Act:**

- Execute module loader

**Assert:**

- Exception is thrown
- Failure is logged clearly

---

## 3. Non-Functional Validation Points

### 3.1 Startup Performance

- Validate assembly scanning overhead remains minimal.
- Measure impact of module discovery on application startup time.

### 3.2 Maintainability

- Verify module registration remains simple and predictable.
- Ensure new modules can be added without Host modifications.

### 3.3 Observability

- Verify module loading events are traceable through logging.

---

## 4. Test Data

### Inputs

- Assemblies containing module implementations
- Multiple feature modules
- Test module implementations
- Misconfigured module scenarios

### Expected Outputs

- Modules discovered automatically
- Services registered successfully
- Host remains decoupled from modules
- Errors reported when module registration fails

---

## 5. TDD Execution Plan

### 1. RED

- Write failing tests for module discovery.
- Write tests validating module registration execution.

### 2. GREEN

- Implement `ModuleLoader`.
- Implement `AddModules()` extension method.

### 3. REFACTOR

- Extract assembly scanning logic.
- Improve diagnostics and logging.
- Optimize module discovery performance.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Architecture rules validated.
- [ ] Modules load automatically during application startup.
- [ ] Host has no direct dependency on feature modules.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Application.UnitTests/
 │       └── Modules/
 │           ├── ModuleLoaderDiscoveryTests.cs
 │           ├── ModuleLoaderRegistrationTests.cs
 │           └── ModuleLoaderDuplicateTests.cs
 │
 ├── IntegrationTests/
 │   └── Hector.Modules.IntegrationTests/
 │       └── ModuleRegistrationIntegrationTests.cs
 │
 └── ArchitectureTests/
└── Hector.ArchitectureTests/
├── ModuleIdentityArchitectureTests.cs
└── HostModuleIsolationTests.cs
```

---

## Summary

This test plan validates the automatic module registration mechanism introduced in [ADR‑0037](/docs/adr/0037-introduce-module-loader-for-auto-registration.md).

By ensuring that modules are discovered and registered through the ModuleLoader, the system preserves the architectural principles of the **Modular Monolith**, maintains Host independence, and enables scalable module onboarding without manual configuration.
