---
name: Task Item
about: Create a Task with full traceability and delivery expectations
title: "[TASK] "
labels: ["backlog", "task"]
assignees: []
---

<!-- 
  NOTE: To ensure link traceability, use absolute repository links:
  Example: https://github.com/ehsan26668/hector-modular-ddd-template/blob/main/docs/adr/0000-example.md
  DO NOT use relative paths like 'docs/adr/...' as they break in GitHub issues.
-->

## Hierarchy & Traceability

- **Parent Feature:** #
- **Related ADR(s):**
  - [ ] <https://github.com/ehsan26668/hector-modular-ddd-template/blob/main/docs/adr/XXXX-title.md>
- **Related Test Plan(s):**
  - [ ] <https://github.com/ehsan26668/hector-modular-ddd-template/blob/main/docs/tests/adr-XXXX-title.md>
- **Related PR(s):** #
- **Target Module / Area:** `src/Modules/ModuleName`

## Goal

Describe the concrete outcome this task must deliver.

## Context

Explain why this task is needed and how it supports the parent feature.

## Implementation Notes

Describe the expected implementation approach, constraints, or important technical considerations.

## Architecture Evaluation (ADR-First Policy)

- [ ] No architecture impact.
- [ ] Architecture impact exists and is covered by a related ADR.
  - **Linked ADR:** <https://github.com/ehsan26668/hector-modular-ddd-template/blob/main/docs/adr/XXXX-title.md>

## Validation Strategy

Describe how this task will be validated in alignment with the project testing standards.

### Automated Verification

- Unit Tests:
- Integration Tests:
- Architecture Tests:

### Manual Verification

-

## Acceptance Criteria

- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3

## Definition of Done (DoD)

- [ ] Implementation follows DDD, Clean Architecture, and repository conventions.
- [ ] Required tests are added/updated and passing in CI.
- [ ] Related ADR / Test Plan / Decision Log is updated and linked with Absolute URLs.
- [ ] Related documentation is updated and synchronized.
- [ ] Pull Request includes complete traceability links (Parent Feature + ADR).
- [ ] Changes satisfy repository governance standards.
- [ ] Self-review is completed before requesting review.

## Notes

Add any dependencies, rollout notes, sequencing details, or implementation considerations.
