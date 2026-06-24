# ADR-0052 — Introduce Result Error Object Model

Status: Accepted

## Context

[ADR‑0047](/docs/adr/ADR-0047-standardize-result-pattern.md) introduced a standardized Result pattern for the Application Layer.

Subsequent ADRs extended this model:

- [ADR‑0048](/docs/adr/ADR-0048-adopt-result-based-validation-handling.md) — Result-based validation handling
- [ADR‑0049](/docs/adr/ADR-0049-adopt-result-based-query-responses.md) — Result-based query responses
- [ADR‑0050](/docs/adr/ADR-0050-establish-application-error-taxonomy.md) — Application error taxonomy
- [ADR‑0051](/docs/adr/ADR-0051-define-allowed-error-categories-for-commands-and-queries.md) — Allowed error categories for commands and queries

However, these ADRs define behavioral rules but do not yet define the concrete error object model used inside

`Result`.

Without a standardized error model:

- modules may invent incompatible error structures
- error codes may be inconsistent
- API error mapping becomes unreliable
- observability and logging lose structure
- architecture tests cannot enforce error consistency

Therefore a canonical Error object model must be introduced.

## Decision

The system introduces a standardized Error model used by Result and `Result<T>`.

All application failures must be represented using this structure.

### Error Structure

An `Error` object must contain the following fields:

```csharp
Error
{
    string Code
    string Message
    ErrorCategory Category
    IReadOnlyDictionary<string, object>? Metadata
}
```

Field meanings:

### Code

A stable machine-readable identifier of the error.

### Message

A human-readable description of the error.

### Category

One of the categories defined in ADR‑0050.

### Metadata

Optional structured data describing additional failure details.

### Error Category Enumeration

The `ErrorCategory` enum must include:

```text
Validation
NotFound
Unauthorized
Forbidden
Conflict
BusinessRule
Infrastructure
Unexpected
```

These values must match the taxonomy defined in [ADR‑0050](/docs/adr/ADR-0050-establish-application-error-taxonomy.md).

Modules must not introduce new categories.

### Result API

Two result types must be defined:

```csharp
Result
Result<T>
```

Minimal API surface:

```csharp
Result.Success()
Result.Failure(Error)

Result<T>.Success(T value)
Result<T>.Failure(Error)
```

Both types must expose:

```csharp
bool IsSuccess
bool IsFailure
Error? Error
```

`Result<T>` must additionally expose:

```csharp
T Value
```

### Validation Errors

Validation failures may contain multiple errors.

Therefore validation errors must include metadata such as:

```csharp
{
  "field": "Name",
  "attemptedValue": "...",
  "validationRule": "NotEmpty"
}
```

The validation pipeline (ADR‑0048) aggregates these errors and returns a structured validation error.

### Exception Mapping

Exceptions must be translated into Error objects according to [ADR‑0050](/docs/adr/ADR-0050-establish-application-error-taxonomy.md):

DomainException → BusinessRule

FluentValidationException → Validation

DbUpdateConcurrencyException → Conflict

External dependency failures → Infrastructure

Unhandled exceptions → Unexpected

Exception translation must occur in the Application Layer pipeline.

### Layer Rules

Domain Layer:

- must not reference Result
- must not reference Error

Domain failures are expressed via exceptions and translated later.

Application Layer:

- produces Result responses
- maps exceptions to errors

API Layer:

- maps ErrorCategory to HTTP responses.

## Consequences

### Positive

- Provides a canonical error contract for the entire system
- Enables consistent Result implementation
- Simplifies API error mapping
- Improves observability and structured logging
- Enables architecture tests for Result usage

### Negative

- Requires all modules to use the shared error model
- Slightly increases verbosity when constructing errors

---
