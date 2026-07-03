# ADR‑0054 — Adopt Result‑Based Error Handling Architecture

## Status

Accepted

## Context

The application layer must provide a consistent, predictable, and structured mechanism for representing operation outcomes.

Traditional exception‑driven error handling introduces several issues:

- Exceptions are frequently used for expected control flow.
- Error semantics become inconsistent across commands and queries.
- API layers must interpret heterogeneous exception types.
- Error responses become unstable and difficult for clients to rely on.

In a modular monolith architecture with CQRS, commands and queries represent explicit application operations. These operations must communicate both successful outcomes and expected failures in a structured and deterministic way.

To address these concerns, the system adopts a Result‑based error handling architecture for all application operations.

This architecture standardizes:

- how application operations communicate success or failure
- how validation failures are represented
- how query responses handle missing data
- how application errors are categorized
- how error objects are structured
- how error codes are defined

The detailed decisions that form this architecture are captured in the following ADRs:

- [ADR‑0047](/docs/adr/ADR-0047-standardize-result-pattern.md) — Standardize Result Pattern for Application Layer
- [ADR‑0048](/docs/adr/ADR-0048-adopt-result-based-validation-handling.md) — Adopt Result‑Based Validation Handling
- [ADR‑0049](/docs/adr/ADR-0049-adopt-result-based-query-responses.md) — Adopt Result‑Based Query Responses
- [ADR‑0050](/docs/adr/ADR-0050-establish-application-error-taxonomy.md) — Establish Application Error Taxonomy
- [ADR‑0051](/docs/adr/ADR-0051-define-allowed-error-categories-for-commands-and-queries.md) — Define Allowed Error Categories for Commands and Queries
- [ADR‑0052](/docs/adr/ADR-0052-introduce-result-error-object-model.md) — Introduce Result Error Object Model
- [ADR‑0053](/docs/adr/ADR-0053-define-result-factory-and-standard-error.md) — Define Result Factory and Standard Error Codes

Together these ADRs define the complete error‑handling model used across the application layer.

---

## Decision

The system adopts a Result‑based error handling architecture for all Application Layer operations.

Application commands and queries must communicate their outcomes using a standardized Result abstraction rather than relying on exceptions for expected failures.

### Core Principles

1. Application operations return Result types

    All command and query handlers must return either:

    - `Result`
    - `Result<TResponse>`

    Expected failures must be represented using `Result.Failure`.

    Unexpected failures must be converted to an `Unexpected` error.

2. Validation failures are returned as Results

    Validation errors must not throw exceptions during normal operation.

    Instead they are returned as:

    ```csharp
    Result.Failure(ValidationError)
    ```

    Validation occurs in the application pipeline before handler execution.

3. Query responses follow explicit missing‑data rules

    Query handlers must use `Result<TResponse>` and follow the standard not‑found policy:

    - Single required resource → `Failure(NotFound)`
    - Collection queries → `Success(empty collection)`
    - Optional resource → `Success(null)`

4. All errors belong to a standardized taxonomy

    Application errors must belong to one of the predefined categories:

    - Validation
    - NotFound
    - Unauthorized
    - Forbidden
    - Conflict
    - BusinessRule
    - Infrastructure
    - Unexpected

    These categories provide a consistent semantic model for failures across the system.

5. Errors follow a standardized object model

    Failures returned from application operations must contain a structured `Error` object including:

    - stable error code
    - human‑readable message
    - error category
    - optional metadata

    This ensures consistent error propagation across application boundaries.

6. Error creation follows standardized conventions

    Errors must be created using centralized factories and follow a standardized error code format.

    Error codes must be:

    - stable
    - machine readable
    - independent from localized messages

7. Exceptions are reserved for unexpected failures

    Exceptions remain the mechanism for representing unexpected failures within the application.

    However:

    - exceptions must not be used for expected control flow
    - exceptions must not escape the application layer
    - unexpected exceptions are converted to `Unexpected` errors

---

### Architectural Flow

The high‑level flow for application operations is:

```text
Client Request
      │
      ▼
Application Pipeline
      │
      ├─ Correlation Behavior
      ├─ Inbox Behavior
      ├─ Validation Behavior
      ├─ Transaction Behavior
      │
      ▼
Command / Query Handler
      │
      ▼
Result<T>
      │
      ▼
API Layer
      │
      ▼
HTTP Response
```

All expected failures are represented as `Result.Failure`.

Unexpected exceptions are converted to `Unexpected` errors before leaving the application layer.

---

## Consequences

### Positive

- Consistent error semantics across the system
- Clear separation between expected failures and exceptional conditions
- Predictable API error contracts
- Improved observability and logging
- Simplified handler implementations
- Architecture rules that can be enforced via automated tests

### Negative

- Additional abstractions (Result, Error, factories)
- Developers must follow error code conventions
- Slightly more verbose handler implementations

---

### Scope

This architecture applies to:

- Application Layer command handlers
- Application Layer query handlers
- Application pipeline behaviors
- API error translation

It does not apply to the Domain Layer, which continues to express failures through domain exceptions and invariant enforcement.

---
