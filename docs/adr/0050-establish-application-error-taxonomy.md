# ADR-0050: Establish Application Error Taxonomy

Status: Accepted

## Context

Once commands and queries return Result, the system requires a consistent classification of failures.

Without a standardized taxonomy:

- different modules invent different error categories
- API mappings become inconsistent
- observability becomes unreliable
- failure handling logic becomes fragmented
- error contracts become unstable over time
- automated architecture rules cannot reliably validate error usage

A centralized error taxonomy ensures consistent semantics across the entire application.

## Decision

The Application Layer defines the following standard error categories:

- Validation
- NotFound
- Unauthorized
- Forbidden
- Conflict
- BusinessRule
- Infrastructure
- Unexpected

Every `Result.Failure` must include an `Error` with a defined category.

Modules must not introduce additional categories.

## Error Catalog

Application errors must be declared in centralized error catalog classes.

Errors must not be created ad-hoc inside command handlers, query handlers, validators, or application services.

The standard catalog structure is:

```text
Errors
├── Commands
│   ├── Projects
│   ├── Users
│   └── ...
│
├── Queries
│   ├── Projects
│   ├── Users
│   └── ...
│
└── Shared
├── Authentication
├── Authorization
└── ...
```

### Command Error Catalog

Errors used by command handlers must be declared under:

```text
Errors.Commands.*
```

Command error category policies are defined by [ADR-0051](/docs/adr/ADR-0051-define-allowed-error-categories-for-commands-and-queries.md).

### Query Error Catalog

Errors used by query handlers must be declared under:

```text
Errors.Queries.*
```

Query error category policies are defined by [ADR-0051](/docs/adr/ADR-0051-define-allowed-error-categories-for-commands-and-queries.md).

### Shared Error Catalog

Errors that are not specific to command or query execution may be declared under:

```text
Errors.Shared.*
```

Shared errors should be used only for cross-cutting concerns such as authentication, authorization, infrastructure, or framework-level failures.

### Error Codes

Each error must contain:

- a stable error code
- a human-readable message
- a defined category
- optional metadata

Error codes are part of the application contract.

Therefore:

- error codes must be unique
- error codes must remain stable after release
- error codes must follow the standard naming convention
- error codes must not be reused for different meanings

The standard naming convention is:

```text
MODULE_REASON
```

Examples:

```text
PROJECT_NOT_FOUND
PROJECT_ALREADY_EXISTS
USER_EMAIL_INVALID
AUTHENTICATION_REQUIRED
```

Error codes must match the following pattern:

```regex
^[A-Z]+(_[A-Z]+)+$
```

### Exception Mapping

The following exception mappings must be applied:

```text
DomainException                 → BusinessRule
FluentValidationException       → Validation
DbUpdateConcurrencyException    → Conflict
External dependency failures    → Infrastructure
Unhandled exceptions            → Unexpected
```

Domain exceptions represent domain invariant violations and must not leak directly to API responses.

They must be translated into `Error` instances before being returned as failures.

### API Mapping

Error categories must map consistently to HTTP status codes:

```text
Validation      → 400
NotFound        → 404
Unauthorized    → 401
Forbidden       → 403
Conflict        → 409
BusinessRule    → 422
Infrastructure  → 503
Unexpected      → 500
```

The Web layer is responsible for translating `ErrorCategory` values into HTTP responses.

The Application layer must not depend on [ASP.NET](https://dotnet.microsoft.com/en-us/apps/aspnet) Core, HTTP abstractions, `ProblemDetails`, or Web-specific result mappers.

### Architecture Enforcement

The following architecture rules must be enforced by tests:

- all `Error.Code` values must be unique
- all `Error.Code` values must follow the naming convention
- all errors must be declared in centralized catalog classes
- command errors must be declared under `Errors.Commands.*`
- query errors must be declared under `Errors.Queries.*`
- command/query category policies must follow ADR-0051
- error contracts must be protected by snapshot/stability tests

## Consequences

### Positive

- Consistent error classification
- Stable API error semantics
- Improved observability and monitoring
- Better contract stability for clients
- Easier architecture enforcement through automated tests
- Clear distinction between command and query failure semantics

### Negative

- Developers must choose the correct error category
- Developers must declare errors in the correct catalog
- Changes to error codes require more discipline because they are treated as contracts

---
