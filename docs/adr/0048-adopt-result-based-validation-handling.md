# ADR-0048 — Adopt Result-Based Validation Handling

Status: Accepted

## Context

Validation of commands and queries is implemented using pipeline behaviors.

Traditionally, validation libraries throw exceptions such as:

```text
FluentValidation.ValidationException
```

However, throwing exceptions for expected validation failures introduces several problems:

- validation errors become indistinguishable from unexpected failures
- exception-based control flow increases complexity
- exception handling logic leaks into higher layers
- handlers may accidentally execute even when validation fails

Since [ADR‑0047](/docs/adr/ADR-0047-standardize-result-pattern.md) introduced a Result pattern for the Application Layer, validation failures must integrate into the same Result flow.

## Decision

Validation failures must be represented using Result failures, not exceptions.

The validation pipeline must:

1. Execute all registered validators.
2. Aggregate validation errors.
3. If validation fails, return:

```text
Result.Failure(ValidationError)
```

or

```text
Result<T>.Failure(ValidationError)
```

The request handler must not execute when validation fails.

### Validation Error Structure

Validation errors must be structured and include:

- error code
- error message
- property name
- validation details

This allows API layers to produce structured validation responses.

### Pipeline Order

The application pipeline must execute in the following order:

1. Correlation behavior
2. Inbox behavior
3. Validation behavior
4. Transaction behavior
5. Handler execution

This ensures that:

- validation occurs before business logic
- invalid requests never reach handlers
- failures are returned consistently through Result

### Exception Handling

Validation logic must not throw exceptions.

If a validator throws unexpectedly, it is treated as an Unexpected failure and handled by the exception pipeline.

## Consequences

### Positive

- Validation integrates naturally with the Result pattern
- No exception-based control flow for expected failures
- Handlers only execute when requests are valid
- Enables consistent API error responses

### Negative

- Requires validators to return structured validation information

---
