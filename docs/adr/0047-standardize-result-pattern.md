# ADR-0047 — Standardize Result Pattern for Application Layer

## Status

Accepted

## Context

Application command and query handlers in the system currently return inconsistent response types such as:

```text
Task<Guid>
Task<bool>
Task<ProjectId>
Task<Unit>
Task<Result>
Task<Result<T>>
```

This inconsistency makes error handling, observability, and API mapping difficult.

Additionally, failures in application workflows may occur for several reasons:

- validation failures
- domain rule violations
- missing resources
- authorization failures
- infrastructure problems
- unexpected exceptions

Without a consistent response model, these failures are handled inconsistently across handlers.

A standardized result model is required to:

- provide a consistent success/failure contract
- prevent leaking exceptions from the Application Layer
- enable structured error classification
- support observability and consistent API mapping

This ADR introduces a unified Result pattern for the Application Layer.

## Decision

All Application Layer operations must return a Result-based response.

Command handlers must return:

```text
Result

or

Result<T>
```

Query handlers must return:

```text
Result<TResponse>
```

The Result abstraction represents:

- successful execution
- expected failures represented as structured errors

### Failure Handling Rules

Expected failures must be represented as:

```text
Result.Failure(Error)
Result<T>.Failure(Error)
```

Unexpected failures must not propagate as raw exceptions.

Unhandled exceptions must be caught by the application pipeline and converted to:

```text
Result.Failure(UnexpectedError)
```

### Layer Boundaries

The Domain Layer must not depend on the Result abstraction.

Domain logic communicates failures via:

- DomainException
- BusinessRuleViolationException

These exceptions are translated in the Application Layer into Result failures.

### Goals

The Result pattern standardizes:

- error propagation
- handler response contracts
- failure classification
- API mapping
- observability

## Consequences

### Positive

- Consistent application response model
- No exception leakage from the Application Layer
- Enables structured error taxonomy (ADR‑0050)
- Supports Result-based validation (ADR‑0048)
- Enables Result-based query responses (ADR‑0049)

### Negative

- Slight increase in handler boilerplate
- Requires developers to explicitly model failures

---
