# ADR 0020: Adopt One DbContext per Feature Module

## Status

Accepted

## Context

Hector follows a Modular Monolith architecture where each feature module represents a cohesive business capability. According to ADR-0017, modules are designed to be isolated units that own their domain model, application logic, infrastructure implementation, and contracts.

When integrating persistence with EF Core, a key architectural question arises: Should the system use a single shared DbContext for all modules, or should each module own its own DbContext?

Using a shared DbContext across modules introduces tight coupling, leads to bloated persistence models, and complicates migrations. To preserve module boundaries and allow for potential future extraction into microservices, the persistence layer must be modularized.

## Decision

Each feature module will define and own its own EF Core DbContext.

The DbContext will be located inside the module's Infrastructure project and will only manage entities belonging to that specific module.

Key implementation details:

- Each module's DbContext will use a dedicated database schema (e.g., `projects.Projects`).
- Cross-module data access via direct DbContext injection is strictly forbidden.
- Modules must interact only through public contracts or asynchronous messaging (Domain/Integration Events).
- All module-specific DbContexts should inherit from a common `BaseDbContext` (defined in BuildingBlocks) to benefit from shared features like Domain Event dispatching and automated `StronglyTypedId` mapping.

Example structure:

    // Within Hector.Modules.Projects.Infrastructure
    public sealed class ProjectsDbContext : BaseDbContext
    {
        public DbSet<Project> Projects => Set<Project>();
        // ... mapping and schema configuration
    }

## Consequences

Positive:

- **Strong Encapsulation:** Modules are decoupled at the database level.
- **Maintainability:** Smaller, more focused DbContexts are easier to manage and optimize.
- **Scalability:** Simplifies the process of moving a module to a separate database or service if needed.
- **Independent Migrations:** Schema changes in one module do not affect others.

Negative:

- **Migration Overhead:** Managing multiple DbContexts requires more discipline during CI/CD.
- **Transaction Complexity:** Distributed transactions (or Outbox pattern) are needed for cross-module consistency.
- **Redundancy:** Some shared configuration might be duplicated across modules.
