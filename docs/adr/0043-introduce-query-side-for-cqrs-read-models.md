# ADR 0043: Introduce Query Side for CQRS Read Models

## Status

Implemented

## Context

The system adopts CQRS and provides an internal mediator implementation within Hector.BuildingBlocks.Application.

Command handling and the domain-driven write model are already standardized across modules. Aggregates enforce business rules and emit domain events that are persisted and eventually published through the transactional outbox mechanism.

However, the architecture currently does not define a standardized approach for implementing the query side of CQRS.

Without an explicit convention, modules may implement read operations inconsistently. This may lead to:

- direct exposure of domain aggregates in read models
- inconsistent query handler structures
- unnecessary aggregate loading for simple read operations
- performance degradation in read-heavy scenarios
- duplicated data access patterns across modules

Given that most production systems are read-heavy, it is important to establish a clear and consistent strategy for implementing read models.

## Decision

The architecture adopts a Read Model Query Strategy where queries bypass the domain model and directly access persistence models using EF Core projections.

Queries are implemented in the Application layer of each module and return dedicated DTOs representing read models.

The following conventions apply.

Query location:

```text
Modules

└── ModuleName

└── Application

└── Queries
```

Typical structure:

```text
Queries

├── GetProjectByIdQuery.cs

├── GetProjectByIdQueryHandler.cs

└── ProjectDto.cs
```

Queries follow the mediator abstractions defined in Hector.BuildingBlocks.Application.

Example:

```csharp
public sealed record GetProjectByIdQuery(ProjectId Id) : IQuery<ProjectDto>;

public sealed class GetProjectByIdQueryHandler : IQueryHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly ProjectsDbContext _dbContext;

    public GetProjectByIdQueryHandler(ProjectsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProjectDto> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Projects
            .Where(x => x.Id == request.Id)
            .Select(x => new ProjectDto(
                x.Id,
                x.Name))
            .FirstAsync(cancellationToken);
    }
}
```

Query design rules:

1. Query handlers must not load aggregates.
2. Query handlers must not use domain repositories.
3. Query handlers access the module DbContext directly.
4. Queries return DTO read models, not domain entities.
5. Queries must not modify state.
6. Query handlers remain inside the Application layer.

This ensures a clean separation between write models (Domain) and read models (Application queries).

## Consequences

### Positive

- clear separation between write and read models
- improved performance for read-heavy workloads
- prevents accidental leakage of domain entities into API contracts
- consistent query implementation across modules
- enables optimized database projections

### Negative

- read models duplicate some domain data
- DTO projections must be maintained manually
- query logic depends on EF Core infrastructure
