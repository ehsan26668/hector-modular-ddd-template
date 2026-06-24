# ADR-0049: Adopt Result-Based Query Responses

Status: Accepted

## Context

Queries represent read operations in the CQRS architecture.

Historically, query handlers often returned raw values such as:

- DTOs
- primitive values
- collections
- nullable values

However, queries may also fail for reasons such as:

- authorization failures
- Application Layer, queries must also follow the same model.

## Decision

All query handlers failures consistently.

Since [ADR‑0047](/docs/adr/ADR-0047-standardize-result-pattern.md) standardizes the Result pattern for the Application Layer, queries must also follow the same model.

All query handlers must return:

```text
Result<TResponse>
```

Query handlers must not return:

- raw DTOs
- primitive values
- collections
- nullable responses

### Query NotFound Policy

The meaning of “not found” depends on query semantics.

Rules:

Single required resource:

```text
Result.Failure(NotFound)
```

Example: GetProjectById

Collection queries:

```text
Result.Success(empty collection)
```

Example: ListProjects

Optional resource queries:

```text
Result.Success(null)
```

Example: FindProjectBySlug

### Allowed Query Failures

Queries may fail with the following categories:

- Validation
- NotFound
- Unauthorized
- Forbidden
- Infrastructure
- Unexpected

Queries should not normally produce:

- BusinessRule
- Conflict

These restrictions are formalized in [ADR‑0051](/docs/adr/ADR-0051-define-allowed-error-categories-for-commands-and-queries.md).

### Exception Handling

Infrastructure exceptions such as:

- SqlException
- TimeoutException
- NullReferenceException

must not escape the Application Layer.

They must be translated to Result failures.

## Consequences

### Positive

- Queries follow the same Result model as commands
- Failures are explicit and structured
- Consistent error handling across the Application Layer

### Negative

- Query handlers must explicitly construct Result responses

---
