# ADR 0044: Package Hector as a .NET SDK Template

## Status

Proposed

## Context

The Hector project is intended to serve as a reusable Modular Monolith + DDD template for new applications.

Currently, when a developer wants to start a new project using Hector, the common workflow is:

- Clone the Hector repository.
- Rename the solution, projects, folders, and namespaces.
- Replace occurrences of Hector with the new project name.
- Fix broken references if any rename step is missed.

This process introduces several issues:

- It is time‑consuming and repetitive.
- It is error‑prone, especially in large solutions with many projects.
- It creates friction for new adopters of the template.
- It complicates automated generation of new projects.

The .NET ecosystem provides a built‑in template engine that allows reusable project templates to be packaged and installed locally or globally. Using this mechanism, developers can create a new project from a template using a simple command.

Example:

```text
dotnet new console -n MyApp
```

Adopting the same mechanism for Hector would significantly improve the developer experience.

## Decision

Hector will be packaged as a .NET SDK Template using the built‑in .NET template engine.

A `.template.config/template.json` file will be added to the root of the template repository.

This configuration will define metadata for the template and specify the base name (Hector) that should be replaced when generating a new project.

Example configuration:

```json
{

    "$schema": "http://json.schemastore.org/template",
    "author": "Hector",
    "classifications": [
        "DDD",
        "Modular Monolith",
        "CQRS"
    ],

    "identity": "Hector.Template.ModularDDD",
    "name": "Hector Modular DDD Template",
    "shortName": "hectorddd",
    "sourceName": "Hector",
    "preferNameDirectory": true,
    "tags": {
        "language": "C#",
        "type": "project"
    }
}
```

After installation, developers can create a new solution using:

```text
dotnet new hectorddd -n ProjectPilot
```

The template engine will automatically replace occurrences of Hector in:

- solution names
- project names
- namespaces
- file names
- folder names
- code references

This removes the need for manual renaming and provides a consistent and reliable project generation process.

## Consequences

### Positive

- Significantly improves developer experience when starting new projects.
- Eliminates the need for manual rename operations.
- Reduces the risk of broken references caused by incomplete renames.
- Aligns Hector with standard .NET project scaffolding practices.
- Enables easier distribution of the template in the future (NuGet template packages or private feeds).
- Allows automated generation of sample or test projects.

### Negative

- Requires maintaining .template.config/template.json.
- Some files may need exclusion rules if certain names must not be replaced.
- Contributors must ensure new files follow the sourceName convention so replacements work correctly.
