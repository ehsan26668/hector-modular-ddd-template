# Test Plan: ADR-0044 Package Hector as a .NET SDK Template

## Status

Accepted

## Context

This test plan validates the **.NET SDK Template packaging strategy** described in [ADR-0044](/docs/adr/0044-package-hector-as-a-dotnet-sdk-template.md).

This ADR standardizes how Hector is distributed and instantiated as a reusable Modular Monolith + DDD solution template using the built-in .NET template engine.

Previously, creating a new solution required manually cloning and renaming the repository structure. That process was error-prone, repetitive, and difficult to automate.

The template packaging mechanism must ensure that:

- template installation succeeds correctly
- project generation works through `dotnet new`
- all occurrences of `Hector` are replaced consistently
- generated solutions compile successfully
- namespaces, project names, and references remain valid
- excluded files are preserved correctly when required

Because the template becomes the foundation for all newly generated solutions, validating correctness and replacement consistency is critical to developer experience and long-term maintainability.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Validate template configuration parsing and replacement rules where applicable.
  - Target Project: `tests/UnitTests/Hector.Template.UnitTests`

- **Integration Tests:**
  - Validate template installation, project generation, restore, and build behavior using the .NET CLI.
  - Target Project: `tests/IntegrationTests/Hector.Template.IntegrationTests`

- **Architecture / Validation Tests:**
  - Validate generated solution structure and naming consistency.
  - Target Project: `tests/ArchitectureTests`

---

## 1. Scope

- **Included:**
  - `.template.config/template.json` validation
  - Template installation through `dotnet new install`
  - Project generation using `dotnet new hectorddd`
  - Source name replacement behavior
  - Namespace replacement validation
  - Solution and project naming consistency
  - Build validation for generated solutions
  - Handling exclusion scenarios

- **Excluded:**
  - NuGet publishing workflows
  - CI/CD packaging pipelines
  - IDE-specific behaviors
  - External distribution feeds

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_InstallTemplateSuccessfully_When_TemplatePackageIsInstalled

**Scenario:**  
The template package is installed using the .NET template engine.

**Arrange:**

- Prepare template source directory containing `.template.config/template.json`.

**Act:**

- Execute:
  `dotnet new install <template-path>`

**Assert:**

- Verify installation completes successfully.
- Verify template appears in `dotnet new list`.
- Verify template short name `hectorddd` is registered.

---

### TC-02: Should_GenerateSolution_When_DotnetNewCommandIsExecuted

**Scenario:**  
A developer generates a new solution using the installed template.

**Arrange:**

- Ensure template is installed.
- Prepare target output directory.

**Act:**

- Execute:
  `dotnet new hectorddd -n ProjectPilot`

**Assert:**

- Verify solution directory is created.
- Verify `.sln` file is generated.
- Verify all expected projects are created successfully.

---

### TC-03: Should_ReplaceSourceName_When_TemplateIsGenerated

**Scenario:**  
The template engine replaces occurrences of `Hector` with the provided project name.

**Arrange:**

- Generate solution using:
  `dotnet new hectorddd -n ProjectPilot`

**Act:**

- Inspect generated files, namespaces, folders, and project names.

**Assert:**

- Verify `Hector` is replaced with `ProjectPilot`.
- Verify namespaces use the new project name.
- Verify project references remain valid after replacement.
- Verify no unintended `Hector` references remain.

---

### TC-04: Should_BuildGeneratedSolution_When_TemplateGenerationCompletes

**Scenario:**  
A generated solution must compile successfully without manual modifications.

**Arrange:**

- Generate a new solution from the template.

**Act:**

- Execute:
  `dotnet restore`
- Execute:
  `dotnet build`

**Assert:**

- Verify restore succeeds.
- Verify build succeeds without compilation errors.
- Verify no broken project references exist.

---

### TC-05: Should_PreserveExcludedFiles_When_ReplacementIsNotRequired

**Scenario:**  
Certain files or tokens may require exclusion from automatic replacement.

**Arrange:**

- Configure exclusion rules or static content inside template files.

**Act:**

- Generate template output.

**Assert:**

- Verify excluded content remains unchanged.
- Verify replacement engine does not alter protected values.
- Verify generated output remains valid.

---

### TC-06: Should_GenerateValidProjectStructure_When_TemplateIsCreated

**Scenario:**  
Generated solutions must preserve the expected modular structure.

**Arrange:**

- Generate a new solution from the template.

**Act:**

- Inspect generated directory structure.

**Assert:**

- Verify modular folder structure exists.
- Verify expected layers are generated:
  - Domain
  - Application
  - Infrastructure
  - Presentation
- Verify test projects are generated correctly.

---

### TC-07: Should_HandleMultipleGenerations_WithoutCrossContamination

**Scenario:**  
Multiple solutions are generated sequentially from the same installed template.

**Arrange:**

- Install template once.

**Act:**

- Generate:
  - `ProjectAlpha`
  - `ProjectBeta`

**Assert:**

- Verify each generated solution uses its own project name.
- Verify no namespace contamination exists between outputs.
- Verify both generated solutions build independently.

---

## 3. Non-Functional Validation Points

### 3.1 Developer Experience

- Verify template generation requires minimal manual steps.
- Verify generated solutions are immediately usable.
- Verify naming conventions remain intuitive and consistent.

### 3.2 Reliability

- Verify generated solutions contain no broken references.
- Verify replacement behavior is deterministic across executions.
- Verify repeated generation produces consistent outputs.

### 3.3 Maintainability

- Verify template configuration remains easy to extend.
- Verify newly added projects follow `sourceName` replacement conventions.
- Verify modular structure consistency is preserved over time.

---

## 4. Test Data

- **Inputs:**
  - Template source directory
  - Project name `ProjectPilot`
  - Project name `ProjectAlpha`
  - Project name `ProjectBeta`

- **Expected Outputs:**
  - Installed SDK template
  - Generated modular solution
  - Successful namespace replacement
  - Successful solution build
  - Consistent project structure

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED**
   - Write failing tests for template installation.
   - Write failing tests for source name replacement.
   - Write failing tests for generated solution build validation.

2. **GREEN**
   - Implement `.template.config/template.json`.
   - Configure `sourceName` replacement.
   - Ensure generated solutions compile successfully.

3. **REFACTOR**
   - Simplify template configuration.
   - Add exclusion rules where necessary.
   - Improve generated project consistency.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Generated solutions compile successfully.
- [ ] Replacement consistency validated.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Template.UnitTests/
 │       └── TemplateConfigurationTests.cs
 ├── IntegrationTests/
 │   └── Hector.Template.IntegrationTests/
 │       ├── TemplateInstallationTests.cs
 │       ├── TemplateGenerationTests.cs
 │       └── TemplateBuildValidationTests.cs
 └── ArchitectureTests/
     └── GeneratedSolutionStructureTests.cs
```

## Summary

This test plan ensures that ADR-0044 correctly packages Hector as a reusable .NET SDK Template.

It validates:

- template installation
- automated project generation
- deterministic source-name replacement
- generated solution integrity
- structural consistency across modules

The result is a reliable and production-ready developer onboarding experience aligned with standard .NET template practices.
