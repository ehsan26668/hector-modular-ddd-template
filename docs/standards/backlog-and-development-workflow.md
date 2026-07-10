# Backlog Governance and Development Workflow

## Purpose

This document defines the standard workflow for managing backlog items and executing development work in a traceable, architecture-aware, and test-driven manner.

It establishes how Epics, Features, and Tasks are structured; when an ADR is required; how decisions are validated; and how work items, pull requests, tests, and documentation remain linked throughout delivery.

This standard complements, and must be used alongside, the following references:

- [ADR Template](../templates/adr-template.md)
- [Test Plan Template](../templates/test-plan-template.md)
- [Decision Log](../decisions/decision-log.md)
- [Testing Standards](./testing-standards.md)
- [Architecture Overview](../architecture/architecture-overview.md)
- [ADR-0036: Architecture Guard Tests](../adr/0036-architecture-guard-tests.md)
- [ADR-0056: Architecture Testing DSL and Rule Builder](../adr/0056-architecture-testing-dsl-and-rule-builder.md)

## Scope

This standard applies to all backlog items and development work in the repository, including:

- Product planning and backlog refinement
- Feature delivery and technical implementation
- Architectural and cross-cutting changes
- Pull request preparation and review
- Validation through tests, test plans, and CI workflows
- Documentation updates required to preserve traceability

This document applies to all contributors and maintainers working on the codebase.

## Core Rules

The following rules are mandatory:

1. Every implementation change must originate from a tracked backlog item.
2. Every backlog item must be small enough to be understood, reviewed, and validated.
3. Changes that affect architecture, module boundaries, contracts, testing strategy, or long-lived technical direction must be documented with an ADR before implementation.
4. Every ADR must be linked to a corresponding Test Plan.
5. Every ADR must be registered in the [Decision Log](../decisions/decision-log.md).
6. Every pull request must link the work item it implements and any related ADR, Test Plan, or standards document.
7. All code changes must follow the project testing strategy defined in [Testing Standards](./testing-standards.md).
8. Work is not considered complete until code, tests, and required documentation are aligned.

## Work Breakdown Model (Epic / Feature / Task)

### Epic

An Epic represents a significant business or technical initiative that spans multiple Features.

Use an Epic when the work:

- requires multiple deliverables
- spans multiple pull requests
- includes multiple Features or architectural threads
- needs roadmap-level tracking

### Feature

A Feature represents a deliverable capability within an Epic.

A Feature should:

- produce a meaningful outcome
- have clear acceptance criteria
- identify architectural impact
- be decomposable into implementation Tasks

A Feature is the preferred level for linking business intent to technical execution.

### Task

A Task represents a concrete unit of execution required to complete a Feature.

A Task should:

- be small enough for a focused pull request
- have a clear outcome
- identify whether code, tests, docs, or architecture artifacts are required
- be traceable to one parent Feature

### Optional Supporting Work Items

The team may also use the following supporting item types when needed:

- Bug: a defect in existing behavior
- Spike: time-boxed investigation with an explicit output
- Chore: operational or maintenance work with no direct user-facing outcome

These item types must still follow the same traceability and documentation rules when they result in code or architectural change.

## ADR-First Change Policy

An ADR must be created before implementation when a change does any of the following:

- introduces or changes an architectural pattern
- modifies module boundaries or dependency rules
- introduces a new cross-cutting framework or infrastructure approach
- changes public or internal contracts with long-lived consequences
- alters testing architecture or enforcement strategy
- changes repository-level conventions or governance rules
- affects CI quality gates in a way that changes team workflow
- has consequences that future contributors must understand

Use the [ADR Template](../templates/adr-template.md) to create the ADR.

An ADR is typically not required for:

- isolated bug fixes with no architectural consequence
- local refactoring that preserves existing design decisions
- straightforward implementation of an already-approved design
- documentation-only changes with no policy or architectural impact

When there is uncertainty, default to creating an ADR or explicitly documenting why one is not needed.

## Decision Log and Test Plan Rules

Every ADR must have a corresponding validation artifact defined through the [Test Plan Template](../templates/test-plan-template.md).

The minimum required process is:

1. Create or update the ADR.
2. Create or update the matching Test Plan.
3. Add or update the ADR entry in the [Decision Log](../decisions/decision-log.md).
4. Implement the change.
5. add or update the required tests.
6. verify that CI enforces the intended behavior where applicable.

The Decision Log is the central registry for architectural decisions and must remain current.

Each ADR entry should allow a reader to discover:

- the ADR document
- the current status
- the date
- the validation artifact or Test Plan

The Test Plan must explain how the decision will be validated through one or more of the following:

- unit tests
- integration tests
- architecture tests
- template tests
- CI workflow verification
- manual verification, only when automation is not practical

## Backlog Lifecycle

The standard backlog lifecycle is:

1. Proposed
2. Refined
3. Ready
4. In Progress
5. In Review
6. Done

### Proposed

The idea is captured but not yet ready for implementation.

### Refined

The work item has been clarified, scoped, and decomposed enough for planning.

### Ready

The work item satisfies the Definition of Ready.

### In Progress

Implementation has started and active development is underway.

### In Review

The pull request is open and awaiting review, validation, or final documentation updates.

### Done

The item satisfies the Definition of Done, including traceability, tests, and documentation.

Backlog items must not move to `In Progress` if key architectural ambiguity remains unresolved.

## Development Lifecycle

The standard development lifecycle is:

1. Understand the parent Epic, Feature, and Task
2. Confirm architectural impact
3. Create or update ADR if required
4. Create or update Test Plan if required
5. Refine acceptance criteria and validation approach
6. Implement using the project testing standards
7. Add or update tests
8. Update linked documentation
9. Open pull request with required links
10. Address review feedback
11. Merge only after all quality gates pass

Development should follow the TDD-oriented workflow defined in [Testing Standards](./testing-standards.md), especially where domain behavior, framework behavior, or architecture rules are being introduced or changed.

For architectural enforcement, contributors should use the repository architecture-testing approach described in:

- [ADR-0036: Architecture Guard Tests](../adr/0036-architecture-guard-tests.md)
- [ADR-0056: Architecture Testing DSL and Rule Builder](../adr/0056-architecture-testing-dsl-and-rule-builder.md)

## Pull Request Linking Rules

Every pull request must clearly identify what it implements and what validates it.

At minimum, the PR description must link to:

- the parent Epic, if applicable
- the Feature
- the Task or equivalent execution item
- the ADR, if applicable
- the Test Plan, if applicable
- any relevant architecture or standards document when the change depends on them

A pull request should also summarize:

- the problem being solved
- the scope of the change
- the expected impact
- the validation performed
- any follow-up work intentionally left out of scope

If the change affects architecture, the PR must make the traceability chain explicit.

Preferred traceability chain:

`Epic -> Feature -> Task -> ADR -> Test Plan -> Tests -> PR -> CI`

## Definition of Ready

A backlog item is Ready when all of the following are true:

- the problem or goal is clearly described
- the scope is bounded
- the parent relationship is known (`Epic`, `Feature`, `Task`)
- acceptance criteria are defined
- architectural impact has been assessed
- ADR requirement has been determined
- dependencies and blockers are identified
- validation expectations are known
- the item is small enough for practical implementation and review

A Task must not start if it depends on an unresolved architectural decision.

## Definition of Done

A backlog item is Done only when all relevant conditions are satisfied:

- implementation is complete
- acceptance criteria are met
- required tests are added or updated
- tests pass locally or in CI as applicable
- architectural rules are preserved
- ADR is created or updated if required
- Test Plan is created or updated if required
- Decision Log is updated if required
- related standards or architecture documents are updated if required
- pull request links and traceability are complete
- review feedback is resolved
- the change is merged successfully

Code completion alone does not qualify an item as Done.

## Traceability Matrix

| Artifact | Must Link To | Purpose |
| --- | --- | --- |
| Epic | Product or technical initiative | High-level intent and grouping |
| Feature | Epic | Deliverable capability |
| Task | Feature | Executable implementation unit |
| ADR | Feature or Task | Architectural decision record |
| Test Plan | ADR | Validation strategy |
| Tests | Task, ADR, or Test Plan context | Executable verification |
| Pull Request | Task, Feature, ADR, Test Plan | Review and merge traceability |
| Decision Log Entry | ADR and Test Plan | Central decision registry |

The expected direction is that every meaningful change can be traced backward to intent and forward to validation.

## Practical Examples

### Example 1: Standard Feature Implementation Without New Architecture

- Epic: Project Management Foundations
- Feature: Create Project
- Task: Implement create project command flow

Expected workflow:

1. Refine Feature and Task.
2. Confirm no new architectural decision is needed.
3. Implement code and tests using [Testing Standards](./testing-standards.md).
4. Open PR linking Epic, Feature, and Task.
5. Merge after tests pass.

### Example 2: Feature Requiring a New Architectural Rule

- Epic: Architecture Governance
- Feature: Enforce repository-wide architecture rules
- Task: Add reusable architecture rule DSL

Expected workflow:

1. Refine Feature and Task.
2. Determine that the change affects testing architecture and long-lived enforcement strategy.
3. Create ADR using [ADR Template](../templates/adr-template.md).
4. Create matching Test Plan using [Test Plan Template](../templates/test-plan-template.md).
5. Register the ADR in [Decision Log](../decisions/decision-log.md).
6. Implement tests and framework changes.
7. Open PR linking Feature, Task, ADR, Test Plan, and related standards or architecture documents.

Relevant repository examples:

- [ADR-0036: Architecture Guard Tests](../adr/0036-architecture-guard-tests.md)
- [ADR-0056: Architecture Testing DSL and Rule Builder](../adr/0056-architecture-testing-dsl-and-rule-builder.md)

These illustrate how a technical initiative can connect Feature and Task execution to ADRs, Test Plans, architecture tests, and CI enforcement.

### Example 3: Cross-Cutting Governance Update

- Epic: Engineering Workflow Maturity
- Feature: Standardize backlog and development governance
- Task: Add repository workflow standard

Expected workflow:

1. Draft the standard in `docs/standards/`.
2. Link to existing ADR, testing, and architecture references.
3. Open a documentation-focused PR with clear scope boundaries.
4. Merge only after review confirms clarity, consistency, and alignment with existing conventions.

## Exceptions

Exceptions to this standard must be rare, explicit, and documented.

Acceptable reasons may include:

- urgent production fixes
- temporary repository maintenance constraints
- work performed to unblock failing infrastructure or CI
- exploratory investigation that does not yet produce a lasting change

When an exception is used:

1. the reason must be documented in the work item or PR
2. any skipped ADR, Test Plan, or documentation update must be called out explicitly
3. follow-up work must be created when the exception leaves traceability gaps

Exceptions are temporary allowances, not alternate workflow paths.

## Ownership and Maintenance

This document is owned by the maintainers responsible for repository standards, architecture governance, and engineering workflow quality.

Ownership responsibilities include:

- keeping this standard aligned with actual team practice
- updating references when related documents move or change
- refining the workflow as the repository evolves
- ensuring consistency with architecture and testing standards

Changes to this document should be reviewed carefully because they affect how all future work is planned, implemented, reviewed, and validated.

If a proposed update changes long-lived governance expectations, contributors should consider whether that update itself requires an ADR.
