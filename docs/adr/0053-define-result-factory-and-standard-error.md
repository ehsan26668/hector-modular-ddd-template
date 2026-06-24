# ADR-0053 — Define Result Factory and Standard Error Codes

Status: Accepted

## Context

[ADR‑0052](/docs/adr/ADR-0052-introduce-result-error-object-model.md) introduced the canonical Error object model used by `Result` and `Result<T>`.

However, without a standardized construction mechanism:

- developers may manually construct Error objects
- error codes may follow inconsistent naming conventions
- modules may introduce incompatible patterns
- duplication of common error creation logic may occur

To maintain consistency across the entire system, a centralized error factory and error code convention must be defined.

## Decision

The system introduces a centralized error factory pattern together with standard error code conventions.

Error instances must be created using predefined factory methods instead of constructing `Error` objects directly.

Direct instantiation of `Error` should be avoided

---

### Error Code Convention

Error codes must follow this format:

```text
<Module>.<Category>.<ErrorName>
```

Examples:

```text
Projects.Validation.InvalidName
Projects.NotFound.ProjectNotFound
Projects.BusinessRule.ProjectNameAlreadyExists
Projects.Conflict.ProjectVersionConflict
Infrastructure.DatabaseUnavailable
Application.UnexpectedFailure
```

Properties of error codes:

- stable identifiers
- machine-readable
- safe for logging and telemetry
- independent from localized messages

Error codes must never change once released

---

### Error Factory

A static `Errors` factory must be introduced to simplify error creation.

Example structure:

```text
Errors
 ├── Validation
 ├── NotFound
 ├── Unauthorized
 ├── Forbidden
 ├── Conflict
 ├── BusinessRule
 ├── Infrastructure
 └── Unexpected
 ```

Example factory methods:

```csharp
Errors.Validation(string code, string message)

Errors.NotFound(string code, string message)

Errors.BusinessRule(string code, string message)

Errors.Conflict(string code, string message)

Errors.Infrastructure(string code, string message)

Errors.Unexpected(string code, string message)
```

These methods internally construct the `Error` object with the appropriate category.

Example usage inside a handler:

```csharp
return Result.Failure(
    Errors.NotFound(
        "Projects.NotFound.ProjectNotFound",
        "The requested project does not exist."));
```

### Module-Specific Error Definitions

Modules may define domain-specific error helpers.

Example:

```csharp
public static class ProjectErrors
{
    public static Error NotFound(ProjectId id) =>
        Errors.NotFound(
            "Projects.NotFound.ProjectNotFound",
            $"Project '{id}' was not found.");

    public static Error NameAlreadyExists(string name) =>
        Errors.BusinessRule(
            "Projects.BusinessRule.ProjectNameAlreadyExists",
            $"Project name '{name}' already exists.");
}
```

This approach ensures:

- readable handlers
- centralized error definitions
- consistent error codes

---

### Validation Error Codes

Validation errors should follow this format:

```text
<Module>.Validation.<Field>.<Rule>
```

Examples:

```csharp
Projects.Validation.Name.Required
Projects.Validation.Name.MaxLength
Projects.Validation.Description.MaxLength
```

Validation metadata must include:

- field name
- validation rule
- attempted value (optional)

---

### Error Metadata

Factory methods may optionally include metadata:

```csharp
Errors.NotFound(
    code,
    message,
    metadata)
```

Example metadata:

```csharp
{
  "entity": "Project",
  "id": "123"
}
```

Metadata improves:

- structured logging
- observability
- debugging

---

### Layer Rules

Domain Layer

- must not reference `Error`
- must not reference `Errors`
- must throw domain exceptions instead

Application Layer

- constructs errors using the `Errors` factory
- translates exceptions into errors

API Layer

- maps `ErrorCategory` to HTTP status codes (ADR‑0050)

---

## Consequences

### Positive

- consistent error construction across the system
- stable and structured error codes
- improved observability and logging
- cleaner application handlers
- prevents duplication of error creation logic

### Negative

- introduces an additional abstraction layer
- requires developers to follow the defined error code conventions

---
