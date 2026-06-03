# ADR-0002: Initialize Modular Monolith Project Structure

**Status:** Accepted
**Date:** 2026-06-03

## Context

The project requires a scalable architecture that supports Domain-Driven Design (DDD) principles, module isolation, and clean separation of concerns. We need a standardized directory layout to enforce these boundaries physically.

## Decision

We have adopted a Modular Monolith folder structure:

- `src/Framework/`: Contains reusable Building Blocks (Domain, Application, Persistence, etc.).
- `src/Modules/`: Contains business-specific modules, each implementing its own internal Clean Architecture.
- `src/Hosts/`: Contains entry points (API, Worker).
- `tests/`: Categorized by test type (Unit, Integration, Architecture).

## Consequences

- **Pros:** Clear boundaries, improved maintainability, easy path to microservices in the future.
- **Cons:** Slightly more initial setup overhead compared to a flat project structure.

## Alternatives

- Flat structure: Rejected, as it leads to "big ball of mud" and lack of module boundaries.
