# Test Plan: ADR-0055 Unified Error Handling and Exception Shielding

## Status

Implemented

## Context

This test plan validates [ADR-0055](/docs/adr/0055-unified-error-handling-andexception-shielding.md), which defines the unified approach to handling unexpected exceptions inside Hector’s Web layer.

Hector uses the `Result`/`Result<T>` pattern for expected application failures, but unexpected runtime exceptions still occur (infrastructure failures, dependency timeouts, code defects). These must be centrally intercepted and transformed into a sanitized, standardized HTTP `ProblemDetails` response.

Key validation goals:

- unhandled exceptions are caught globally
- unified error contract shape remains consistent
- internal details are never leaked
- observability preserved via `traceId`
- Result-based failures maintain precedence over exception fallback

---

## Test Strategy

### Unit Tests

Validate isolated logic of `GlobalExceptionHandler`:

- exception interception
- HTTP 500 mapping
- sanitized response payload
- traceId propagation
- logging behavior

Target Project:  
`tests/UnitTests/Hector.BuildingBlocks.Web.UnitTests`

### Integration Tests

Validate the real request pipeline:

- end‑to‑end exception shielding
- middleware ordering correctness
- no override of Result-based failures
- actual HTTP output correctness

Target Project:  
`tests/IntegrationTests/Hector.BuildingBlocks.Web.IntegrationTests`

---

## 1. Scope

### Included

- `GlobalExceptionHandler`
- `IExceptionHandler` pipeline integration
- ProblemDetails generation
- traceId propagation
- sanitization rules
- interaction with `ResultEndpointFilter` and `ResultHttpMapper`
- ASP.NET Core request pipeline behavior

### Excluded

- domain/business validation rules
- Result‑based expected failures
- custom exception-to-status mappings beyond ADR‑0055

---

## 2. Test Cases (Unit / Integration)

### TC-01: Should_ReturnTrueAndSetStatusCode500_When_UnhandledExceptionIsHandled

- **Scenario:** A generic unhandled exception is passed to the handler.
- **Arrange:** Create `DefaultHttpContext`, writable stream, generic exception.
- **Act:** Call `TryHandleAsync`.
- **Assert:** Returns `true`; status code = `500`.

### TC-02: Should_WriteStandardProblemDetailsContract_When_UnhandledExceptionOccurs

- **Scenario:** Handler must generate standard unified error contract.
- **Arrange:** HTTP context + thrown exception.  
- **Act:** Invoke handler and read JSON.  
- **Assert:** JSON contains:

  - title: "InternalServerError"
  - status: 500
  - type: "<https://hector/errors/internal-server-error>"
  - detail: "An unexpected error occurred on the server."

### TC-03: Should_NotExposeSensitiveExceptionMessage_When_ExceptionContainsInternalDetails

- **Scenario:** Exception contains sensitive message.
- **Arrange:** Exception with internal DB details.
- **Act:** Invoke handler.
- **Assert:** No sensitive message; detail includes only sanitized message.

### TC-04: Should_IncludeTraceIdInProblemDetailsExtensions_When_ExceptionIsHandled

- **Scenario:** Response must include traceId.
- **Arrange:** HTTP context with known traceId.
- **Act:** Invoke handler.
- **Assert:** traceId exists and equals `HttpContext.TraceIdentifier`.

### TC-05: Should_LogError_When_UnhandledExceptionIsCaptured

- **Scenario:** Exception must be logged.  
- **Arrange:** Substitute logger + exception.  
- **Act:** Invoke handler.  
- **Assert:** `LogError` is called with the original exception.

### TC-06: Should_SetProblemDetailsContentType_When_ResponseIsWritten

- **Scenario:** Response must be written as problem+json.  
- **Arrange:** HTTP context.  
- **Act:** Invoke handler.  
- **Assert:** Content-Type is `application/problem+json`.

---

### Integration Test Cases

### TC-07: Should_ReturnProblemDetailsResponse_When_EndpointThrowsUnhandledException

- **Scenario:** Real endpoint throws exception.  
- **Arrange:** Host app + endpoint throws `InvalidOperationException`.  
- **Act:** Call endpoint.  
- **Assert:** Status = 500, sanitized ProblemDetails, traceId exists.

### TC-08: Should_NotOverrideResultBasedFailure_When_EndpointReturnsFailureResult

- **Scenario:** Endpoint returns `Result.Failure(...)`.  
- **Arrange:** Endpoint uses `ResultEndpointFilter`.  
- **Act:** Send HTTP request.  
- **Assert:** Status matches Result mapping; global exception handler not used.

### TC-09: Should_CatchExceptionsThrownAfterExceptionHandlerRegistration_When_RequestPipelineExecutes

- **Scenario:** Exception occurs after handler registration.  
- **Arrange:** Middleware throws.  
- **Act:** Send request.  
- **Assert:** Global handler processes exception; no raw failure.

### TC-10: Should_PreserveUnifiedErrorContract_When_ExceptionResponseIsReturned

- **Scenario:** Unexpected exceptions must align with unified API error contract.  
- **Arrange:** Throwing endpoint.  
- **Act:** Capture payload.  
- **Assert:** Structure matches the unified error contract baseline.

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

Ensure no leakage of:

- stack trace
- exception type name
- internal server details
- connection strings
- SQL fragments

### 3.2 Observability & Traceability

Ensure:

- full exception logging
- traceId propagation
- ability to correlate logs and requests

### 3.3 Contract Stability

Ensure:

- same structural error contract for exceptions and Result failures
- predictable fields for consumers

---

## 4. Test Data

### Inputs (Exceptions)

- `Critical system failure!`
- `Database connection string 'Server=prod-db;User Id=sa;Password=123' failed`
- `Object reference not set to an instance of an object`
- `Unexpected downstream service timeout`

### Expected Outputs (Sanitized)

An unexpected error occurred on the server.

---

## 5. TDD Execution Plan

1. **RED**  
   Define failing tests:
   - exception shielding
   - sanitization
   - logging
   - traceId
   - precedence Result vs exception

2. **GREEN**  
   Implement minimal production logic:
   - `GlobalExceptionHandler`
   - registration extensions
   - ProblemDetails mapping

3. **REFACTOR**  
   Improve:
   - constants & helpers  
   - pipeline registration  
   - mapping structure

---

## 6. Exit Criteria

- [✅] All unit tests pass  
- [✅] All integration tests pass  
- [✅] No raw exception info leaked  
- [✅] traceId present in responses  
- [✅] Result-based failures maintain correct precedence  
- [✅] Documentation updated  

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   └── Hector.BuildingBlocks.Web.UnitTests/
 │       └── Results/
 │           └── GlobalExceptionHandlerTests.cs
 └── IntegrationTests/
     └── Hector.BuildingBlocks.Web.IntegrationTests/
         └── Results/
             ├── GlobalExceptionHandlingTests.cs
             └── ResultAndExceptionPipelinePrecedenceTests.cs
```

---

## Summary

This test plan verifies ADR‑0055 by ensuring Hector’s Web layer provides robust exception shielding while preserving the unified error contract. The combined unit + integration tests guarantee observability, security, and contract stability across unexpected failures.

---
