# Test Plan: ADR-0045 Publish Hector as an Installable .NET Template

## Status

Accepted

## Context

This test plan validates the **distribution and installation mechanism of the Hector template as a .NET installable template package** described in [ADR-0045](/docs/adr/0045-publish-hector-as-installable-dotnet-template.md).

ADR-0044 introduced the packaging of Hector as a .NET SDK template. However, the template remained usable primarily through local installation or repository cloning.

ADR-0045 extends this capability by enabling Hector to be distributed as a versioned template package that can be installed using the .NET CLI.

The goal is to allow developers to install the template using:

dotnet new install Hector.Template

and create new projects using:

dotnet new hectorddd -n MyApp

Validating this behavior ensures that:

- the template package can be installed correctly
- the template becomes discoverable through the CLI
- project generation works after installation
- versioning and upgrades behave predictably
- template uninstall operations work correctly

Since the template will be used as the entry point for creating new modular DDD systems, installation reliability and deterministic generation are critical.

## Test Strategy

Define the layers of testing to be used:

- **Unit Tests:**
  - Validate template package metadata and configuration consistency.
  - Target Project: `tests/UnitTests/Hector.Template.UnitTests`

- **Integration Tests:**
  - Validate CLI-based installation, template discovery, project generation, and uninstall behavior.
  - Target Project: `tests/IntegrationTests/Hector.Template.IntegrationTests`

- **Architecture / Validation Tests:**
  - Validate package structure and template exposure consistency.
  - Target Project: `tests/ArchitectureTests`

---

## 1. Scope

- **Included:**
  - NuGet packaging validation
  - Template installation using .NET CLI
  - Template discovery via `dotnet new list`
  - Project generation using installed template
  - Version installation and upgrade scenarios
  - Template uninstall behavior

- **Excluded:**
  - CI/CD publishing workflows
  - External NuGet feed configuration
  - IDE-specific installation mechanisms

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_InstallTemplatePackage_When_DotnetNewInstallIsExecuted

**Scenario:**  
The template package is installed using the .NET CLI.

**Arrange:**

- Ensure template package `Hector.Template` is available locally or in a package source.

**Act:**

Execute:

dotnet new install Hector.Template

**Assert:**

- Verify installation completes successfully.
- Verify CLI output confirms template installation.
- Verify no installation errors occur.

---

### TC-02: Should_RegisterTemplateShortName_When_TemplateIsInstalled

**Scenario:**  
After installation, the template should appear in the CLI template list.

**Arrange:**

- Install template package.

**Act:**

Execute:

dotnet new list

**Assert:**

- Verify template appears in the list.
- Verify short name `hectorddd` is registered.
- Verify template metadata matches configuration.

---

### TC-03: Should_GenerateProject_When_TemplateIsInstalled

**Scenario:**  
Developers create a project using the installed template.

**Arrange:**

- Ensure template package is installed.

**Act:**

Execute:

dotnet new hectorddd -n MyApp

**Assert:**

- Verify project directory `MyApp` is created.
- Verify solution and project files are generated.
- Verify namespaces use `MyApp`.

---

### TC-04: Should_BuildGeneratedProject_When_ProjectIsCreatedFromInstalledTemplate

**Scenario:**  
A project generated from the installed template must build successfully.

**Arrange:**

- Generate a new project using the installed template.

**Act:**

Execute:

dotnet restore

and

dotnet build

**Assert:**

- Verify dependency restore succeeds.
- Verify solution builds successfully.
- Verify no broken references exist.

---

### TC-05: Should_InstallSpecificVersion_When_VersionIsProvided

**Scenario:**  
A specific template version is installed using the CLI.

**Arrange:**

- Ensure multiple versions of the template package are available.

**Act:**

Execute:

dotnet new install Hector.Template::1.0.0

**Assert:**

- Verify version 1.0.0 is installed.
- Verify CLI confirms correct version.
- Verify template works after installation.

---

### TC-06: Should_UpgradeTemplate_When_NewVersionIsInstalled

**Scenario:**  
An existing template installation is upgraded to a newer version.

**Arrange:**

- Install version 1.0.0 of the template.

**Act:**

Install a newer version:

dotnet new install Hector.Template::1.1.0

**Assert:**

- Verify the newer version replaces the previous installation.
- Verify template metadata reflects the updated version.
- Verify project generation continues to work.

---

### TC-07: Should_UninstallTemplate_When_UninstallCommandIsExecuted

**Scenario:**  
A developer removes the template from the CLI.

**Arrange:**

- Ensure template is installed.

**Act:**

Execute:

dotnet new uninstall Hector.Template

**Assert:**

- Verify template is removed successfully.
- Verify template no longer appears in `dotnet new list`.
- Verify CLI reports successful uninstall.

---

## 3. Non-Functional Validation Points

### 3.1 Developer Experience

- Verify template installation requires minimal setup.
- Verify project creation commands are intuitive.
- Verify CLI output provides clear installation feedback.

### 3.2 Reliability

- Verify installation works consistently across environments.
- Verify version upgrades do not break existing template functionality.
- Verify uninstall operations do not leave residual state.

### 3.3 Distribution Consistency

- Verify template metadata matches the package version.
- Verify the template short name remains stable across releases.
- Verify template installation produces deterministic results.

---

## 4. Test Data

- **Inputs:**
  - Template package `Hector.Template`
  - Template version `1.0.0`
  - Template version `1.1.0`
  - Project name `MyApp`

- **Expected Outputs:**
  - Successfully installed template
  - Discoverable template via CLI
  - Generated project using `hectorddd`
  - Successful project build
  - Correct version upgrade behavior
  - Successful uninstall operation

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED**
   - Write failing tests for template installation.
   - Write failing tests for version installation.
   - Write failing tests for project generation using installed template.

2. **GREEN**
   - Package template as a NuGet template package.
   - Configure template metadata.
   - Ensure CLI installation works correctly.

3. **REFACTOR**
   - Improve package metadata clarity.
   - Simplify installation instructions.
   - Ensure versioning behavior is predictable.

---

## 6. Exit Criteria

- [ ] All Unit Tests pass.
- [ ] All Integration Tests pass.
- [ ] Template installation validated.
- [ ] Project generation validated.
- [ ] Version upgrade validated.
- [ ] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.Template.UnitTests/
 │       └── TemplatePackageMetadataTests.cs
 ├── IntegrationTests/
 │   └── Hector.Template.IntegrationTests/
 │       ├── TemplateInstallTests.cs
 │       ├── TemplateVersionTests.cs
 │       └── TemplateUninstallTests.cs
 └── ArchitectureTests/
     └── TemplatePackageStructureTests.cs
```

## Summary

This test plan ensures that ADR-0045 successfully enables Hector to be distributed as an installable .NET template package.

It validates:

- template package installation
- CLI-based template discovery
- project generation using the installed template
- template version management
- template uninstall behavior

The result is a reliable and developer-friendly distribution mechanism that allows teams to quickly bootstrap new modular DDD applications using Hector.
