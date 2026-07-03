# ADR-0051: Define Allowed Error Categories for Commands and Queries

## Status

Accepted

## Context

[ADR‑0050](/docs/adr/ADR-0050-establish-application-error-taxonomy.md) introduced a global error taxonomy.

However, not every error category is appropriate for every operation type.

Commands and queries have different semantics:

Commands:

- modify system state
- enforce domain rules
- may encounter concurrency conflicts

Queries:

- read data from read models
- must not execute domain business rules
- should not produce write-related conflicts

Therefore, the allowed error categories must be explicitly defined for each operation type.

## Decision

### Allowed Command Failures

Commands may return failures with the following categories:

- Validation
- Unauthorized
- Forbidden
- NotFound
- Conflict
- BusinessRule
- Infrastructure
- Unexpected

Commands may enforce domain invariants and therefore may produce BusinessRule failures.

Commands may also encounter concurrency conflicts.

### Allowed Query Failures

Queries may return failures with the following categories:

- Validation
- Unauthorized
- Forbidden
- NotFound
- Infrastructure
- Unexpected

Queries must not produce:

- Conflict
- BusinessRule

These categories are considered command-specific.

### Query Failure Semantics

Queries operate on read models.

Typical query failures include:

- missing resources
- authorization failures
- infrastructure problems
- validation errors

Domain rule execution is not part of query processing.

### Enforcement

Architecture tests must ensure:

- Query handlers do not return BusinessRule errors
- Query handlers do not return Conflict errors

These rules ensure strict CQRS separation.

## Consequences

### Positive

- Clear separation of command and query failure semantics
- Prevents domain rule leakage into query logic
- Enables architecture tests to enforce correct behavior

### Negative

Query handlers must avoid domain rule evaluation

---
