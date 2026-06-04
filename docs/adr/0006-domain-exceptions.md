# ADR 0006: Domain Exception Hierarchy

## Status

Accepted

## Context

We need a standardized way to handle business rule violations within the domain layer. Standard exceptions (like `ArgumentException`) are too generic and don't clearly distinguish between technical failures and business domain violations.

## Decision

- Introduce `DomainException` as the base class for all domain-related errors.
- Introduce `BusinessRuleViolationException` as a specific type for invariant failures.
- These exceptions should be thrown by Aggregate Roots or Value Objects when a business rule is broken.

## Consequences

- **Positive:** Enables centralized error handling (e.g., global filter to catch these and return 400 Bad Request).
- **Positive:** Distinguishes clearly between "System Error" (500) and "Business Rule Error" (400).
- **Negative:** Requires mapping these exceptions to API responses in the Application/Presentation layer.
