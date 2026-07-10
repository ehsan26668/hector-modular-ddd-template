# Hector: Modular DDD Template for .NET

![.NET](https://img.shields.io/badge/.NET-10-blue)
![Architecture](https://img.shields.io/badge/Architecture-Modular_DDD-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

Hector is an enterprise-grade, modular architecture template for .NET, designed to help teams build complex systems with clarity, consistency, and architectural discipline.

Rather than being just a starter repository, Hector provides a structured foundation for organizations that want to scale domain-driven design, enforce architectural boundaries, and evolve their systems through explicit engineering standards.

## Why Hector

Hector is built for teams that need more than project scaffolding. It is intended for long-lived systems where maintainability, consistency, and team alignment matter as much as delivery speed.

### Core value propositions

- **Modular DDD by default**  
  Enforces clear bounded contexts and strong module isolation to prevent architecture erosion.

- **Result-oriented application flow**  
  Replaces exception-driven control flow with a functional `Result` pattern for predictable and explicit behavior.

- **Strongly typed domain model**  
  Uses strongly typed IDs and rich domain primitives to reduce ambiguity and eliminate primitive obsession.

- **Reliable integration patterns**  
  Includes Transactional Outbox and Inbox patterns for resilient, idempotent event-driven workflows.

- **Architecture as executable policy**  
  Uses architecture tests to continuously enforce boundaries, conventions, and design decisions.

- **Repository-level engineering governance**  
  Supports explicit standards for testing, architectural decisions, and pull request classification.

## System Architecture

<p align="center">
  <img src="docs/assets/architecture-diagram.png" alt="Architecture Overview" width="600">
</p>

## Example: Result-Based Command Handling

Hector standardizes how success and failure are modeled in the application layer:

```csharp
    public async Task<Result<ProjectId>> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        if (await _repository.ExistsAsync(request.Name))
        {
            return Result.Failure<ProjectId>(ProjectErrors.NameAlreadyExists);
        }

        var project = Project.Create(request.Name);
        await _repository.AddAsync(project);

        return Result.Success(project.Id);
    }
```

## Quality Model

Hector treats quality as part of the architecture, not as a downstream activity.

### Architecture testing

The repository includes automated architecture tests to verify rules such as:

- Layer dependency direction
- Module isolation
- Query and command handling conventions
- Error contract consistency
- Result-layer isolation
- Event contract placement and naming

### Testing strategy

The solution combines multiple test layers:

- **Unit tests** for domain and framework behavior
- **Integration tests** for persistence, web, and module workflows
- **Architecture tests** for structural governance
- **Template tests** to verify installability and template integrity

## Governance & Standards

To keep engineering practices explicit and repeatable, Hector documents repository-level standards and governance rules.

- [Label Taxonomy](docs/standards/label-taxonomy.md)  
  Defines the standard pull request label taxonomy used to classify changes consistently across the repository.

- [Testing Standards](docs/standards/testing-standards.md)  
  Documents expectations for unit, integration, architecture, and template testing practices.

- **Architecture Decision Records (ADR)**  
  Significant technical decisions are documented in `docs/adr` to preserve architectural context over time.

## Architecture Decision Records

Hector evolves through explicit decisions rather than undocumented convention.

Recent ADRs relevant to the current architecture include:

- ADR-0047: Standardize Result Pattern
- ADR-0050: Establish Application Error Taxonomy
- ADR-0054: Adopt Result-based Error Handling Architecture
- ADR-0056: Architecture Testing DSL and Rule Builder

## Intended Audience

Hector is especially suitable for:

- Architects designing modular monoliths
- Senior .NET developers applying DDD in practice
- Teams building internal platforms or reusable foundations
- Organizations that want architectural governance enforced through code

## License

This project is licensed under the MIT License.
