# Label Taxonomy

## Purpose

This document defines the official pull request label taxonomy for the repository.

Its purpose is to make pull requests easier to classify, review, validate, search, and report on.  
Each pull request should communicate two things clearly:

1. What kind of change it introduces.
2. Which part of the system it affects.

This taxonomy is intended to be stable and explicit. If labels change, this document must be updated accordingly.

---

## Label Categories

Pull request labels are grouped into two required categories:

- `type:` labels describe the nature of the change.
- `area:` labels describe the affected part of the codebase.

A pull request is expected to have:

- exactly one `type:` label
- at least one `area:` label

---

## Type Labels

`type:` labels describe the intent of the change.

### Allowed `type:` labels

- `type: feat`  
  Introduces a new feature or extends existing behavior in a meaningful way.

- `type: fix`  
  Corrects a bug, defect, or unintended behavior.

- `type: refactor`  
  Improves internal structure or readability without intentionally changing externally observable behavior.

- `type: test`  
  Adds, updates, or reorganizes tests.

- `type: docs`  
  Changes documentation only.

- `type: chore`  
  Covers maintenance work such as tooling, build configuration, CI updates, dependency management, or repository housekeeping.

### `type:` rules

- A pull request must have exactly one `type:` label.
- Multiple `type:` labels on the same pull request are invalid.
- If a pull request includes several kinds of work, the `type:` label should reflect the primary intent of the change.
- Mixed-purpose pull requests should be avoided when they make classification unclear.

---

## Area Labels

`area:` labels describe which architectural layer, subsystem, or repository area is affected.

### Core architectural areas

- `area: domain`  
  Domain model, business rules, aggregates, value objects, domain services, invariants, and domain events.

- `area: application`  
  Application layer concerns such as commands, queries, handlers, orchestration, validation, and use-case coordination.

- `area: infrastructure`  
  Persistence, external integrations, messaging infrastructure, background processing, serialization, storage, and technical implementations.

- `area: web`  
  Host/API/web entry points, HTTP pipeline, middleware, endpoints, filters, and web-facing composition concerns.

### Testing and quality areas

- `area: architecture-tests`  
  Architecture tests, structural rules, rule DSLs, repository guard rails, and design enforcement tests.

- `area: unit-tests`  
  Unit test changes not primarily tied to architecture test rules.

- `area: integration-tests`  
  Integration or end-to-end style test changes.

### Documentation and repository areas

- `area: docs`  
  ADRs, standards, templates, release notes, vision documents, and other repository documentation.

- `area: ci`  
  CI/CD workflows, automation scripts, validation rules, and pipeline-related changes.

- `area: templates`  
  Project templates, scaffolding, generation rules, or reusable repository templates.

### `area:` rules

- A pull request must have at least one `area:` label.
- A pull request may have multiple `area:` labels if the change genuinely spans more than one area.
- `area:` labels should reflect the actual impacted code or assets, not only the intended audience of the change.
- Broad pull requests that affect many areas should still use only the labels that materially apply.

---

## Label Selection Guidance

Use the smallest set of labels that accurately describes the change.

### Examples

- Add a new command handler in the application layer  
  - `type: feat`
  - `area: application`

- Fix a repository bug in persistence code  
  - `type: fix`
  - `area: infrastructure`

- Update an ADR and a testing standard document  
  - `type: docs`
  - `area: docs`

- Add architecture guard rules and supporting architecture tests  
  - `type: test`
  - `area: architecture-tests`

- Update CI validation logic for pull requests  
  - `type: chore`
  - `area: ci`

- Refactor domain entities and related command handlers  
  - `type: refactor`
  - `area: domain`
  - `area: application`

---

## Pull Request Contract

The repository uses this taxonomy as the canonical contract for pull request classification.

When pull request templates, automation, or validation workflows exist, they should align with this document.  
That includes, where applicable:

- pull request templates that collect change type from authors
- automation that applies labels based on changed paths
- validation workflows that reject missing or invalid labels

If automation is introduced later, it must enforce this taxonomy rather than redefine it.

---

## Maintenance Rules

Changes to the label taxonomy must be made deliberately.

### Required expectations

- New labels must be added to this document before they are used as part of the standard workflow.
- Removed or renamed labels must be reflected here immediately.
- Any pull request template or automation that depends on labels must be updated together with this document.
- If labels are used in reporting or dashboards, downstream consumers should be reviewed before taxonomy changes are merged.

---

## Recommended Mapping Guidance

If the repository uses path-based automation, the following mapping approach is recommended:

- `docs/**` -> `area: docs`
- `.github/workflows/**` -> `area: ci`
- `.template.config/**` -> `area: templates`
- `src/Framework/**/*.Domain/**` or equivalent domain folders -> `area: domain`
- `src/Framework/**/*.Application/**` or equivalent application folders -> `area: application`
- `src/Framework/**/*.Persistence/**` or equivalent infrastructure folders -> `area: infrastructure`
- `src/Framework/**/*.Web/**` or host/web folders -> `area: web`
- `tests/ArchitectureTests/**` -> `area: architecture-tests`
- `tests/UnitTests/**` -> `area: unit-tests`
- `tests/IntegrationTests/**` -> `area: integration-tests`

This mapping is guidance only unless enforced by repository automation.

---

## Non-Goals

This taxonomy does not attempt to:

- replace meaningful pull request titles or descriptions
- describe release-note categories in full detail
- model business priority, severity, or ownership
- replace architectural decision records or implementation documentation

---

## Summary

The repository standard for pull requests is:

- exactly one `type:` label
- at least one `area:` label
- consistent use of labels across templates, reviews, and automation

This document is the reference point for that contract.
