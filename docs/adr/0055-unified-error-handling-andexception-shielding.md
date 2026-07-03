# ADR 0055: Unified Error Handling and Exception Shielding

## Status

Implemented

## Context

The Hector architecture standardizes error handling in the Application layer using the `Result` pattern ([ADR-0047](/docs/adr/0047-standardize-result-pattern.md) through [ADR-0054](/docs/adr/0054-adopt-result-based-error-handling-architecture.md)). Application use cases return `Result` or `Result<T>` to represent expected failures such as validation errors, conflicts, or missing resources.

However, runtime exceptions may still occur due to unexpected conditions such as infrastructure failures, programming errors, or external dependency issues. Without a centralized handling strategy, these exceptions may:

- Leak internal implementation details (stack traces, internal messages)
- Produce inconsistent HTTP responses
- Break the unified error contract expected by API consumers
- Require repetitive try/catch logic in endpoints

To preserve a consistent API contract and protect internal details, a global mechanism is required to convert unhandled exceptions into standardized HTTP responses.

## Decision

Hector adopts a Unified Error Handling strategy where:

1. Expected failures in the Application layer must be represented using `Result` / `Result<T>`.

2. Exceptions are reserved for unexpected failures (infrastructure faults, programming errors, external dependency failures).

3. The Web layer introduces a Global Exception Handler responsible for intercepting unhandled exceptions and converting them into standardized `ProblemDetails` responses.

4. This handler acts as an Exception Shield, preventing internal exceptions from leaking implementation details to API consumers.

5. The resulting HTTP response must follow the same error contract used by `ResultHttpMapper`, ensuring that both Result-based failures and unexpected exceptions produce a consistent API error format.

6. The implementation is based on [ASP.NET](https://dotnet.microsoft.com/en-us/apps/aspnet) Core’s `IExceptionHandler` infrastructure and is registered globally in the HTTP pipeline.

All unhandled exceptions are logged with full details, while the HTTP response contains a sanitized message.

Example pipeline:

```tetx
Endpoint

↓

ResultEndpointFilter

↓

ResultHttpMapper

↓

GlobalExceptionHandler (fallback)

↓

ProblemDetails HTTP response
```

Example sanitized response:

```json
{

    "type": "https://hector/errors/internal-server-error",

    "title": "InternalServerError",

    "status": 500,

    "detail": "An unexpected error occurred on the server.",

    "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00"

}
```

## Consequences

### Positive

- Ensures a single unified error contract for all API responses.
- Prevents leakage of internal exception details.
- Centralizes exception handling logic in the Web layer.
- Improves observability by consistently attaching trace identifiers.
- Keeps Application layer focused on business errors via Result.

### Negative

- Requires careful discipline to ensure business failures are returned as Result rather than exceptions.
- Some debugging scenarios may require checking server logs instead of relying on API responses.
- Adds a global infrastructure component to the Web layer.
