# ADR-0045: Publish Hector as an Installable .NET Template

## Status

Accepted

## Context

Hector has been packaged as a .NET SDK template (ADR-0044), allowing new projects to be created from the repository structure.

However, the template is currently usable only via local installation or by cloning the repository. This approach creates friction for developers who want to start new projects using Hector.

To improve developer experience and enable broader adoption, Hector should be distributed as an installable .NET template package.

This would allow developers to install the template using the .NET CLI and create projects without cloning the repository.

Example desired workflow:

```text
dotnet new install Hector.Template
dotnet new hectorddd -n MyApp
```

## Decision

Hector will be distributed as an installable .NET template package.

The template will be packaged and published so that it can be installed using the .NET CLI template installation mechanism.

The distribution format will be a NuGet package containing the template configuration and project structure.

The package will expose the template using the existing short name:

```text
hectorddd
```

Developers will be able to install the template using:

```cmd
dotnet new install Hector.Template
```

and create new projects using:

```cmd
dotnet new hectorddd -n MyApp
```

## Consequences

### Positive

- Simplifies project creation workflow
- Removes need to clone the Hector repository
- Enables versioned distribution of the template
- Allows teams to standardize project architecture
- Makes Hector usable as a reusable architecture starter kit

### Negative

- Requires packaging and version management
- Requires template release workflow

### Future Work

- Publish template to NuGet
- Add versioning strategy
- Automate template publishing via CI/CD
- Improve CLI help documentation

### Related ADRs

ADR-0044: Package Hector as a .NET SDK template
