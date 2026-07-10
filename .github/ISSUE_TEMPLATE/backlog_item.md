---
name: Backlog Item
about: Create a structured backlog item (Feature or Task) for implementation work
title: "[TASK] "
labels: ["backlog"]
assignees: []
---

## Hierarchy & Traceability

- **Parent Epic:** <!-- Link to Epic issue if applicable -->
- **Parent Feature:** <!-- Link to Feature issue (Required for Tasks) -->
- **Target Module:** <!-- e.g., Projects, Framework, Host, Cross-Module -->

## Goal

Describe the concrete outcome this item must achieve.

## Context

Why is this needed? What business value or technical necessity drives this task?

## Scope

- **In Scope:**
- **Explicitly Out of Scope:**

## Architecture Evaluation (ADR-First Policy)

- [ ] This change **does NOT** impact architecture, contracts, boundaries, or conventions.
- [ ] This change **IMPACTS** architecture (requires an ADR).
  - *Linked ADR:* <!-- Link to ADR file or draft PR -->
  - *Decision Log Status:* <!-- Proposed / Approved -->
  - *Linked Test Plan:* <!-- Link to the corresponding Test Plan -->

## Test Strategy

Explain how this change is validated in alignment with the Project Testing Standards:

- **Unit Tests:**
- **Integration Tests:**
- **Architecture Tests:**
- **Manual Verification:** (Only if automation is not practical)

## Acceptance Criteria

- [ ] Criterion 1
- [ ] Criterion 2
- [ ] Criterion 3

## Definition of Done (DoD) Checklist

- [ ] Code implemented in compliance with DDD and Clean Architecture standards
- [ ] Tests added/updated (Unit, Integration, or Architecture) and passing locally
- [ ] ADR / Test Plan / Decision Log updated and linked (if architecture was impacted)
- [ ] Related documentation (e.g., README, API docs, standards) kept in sync
- [ ] Pull Request follows linking rules (Epic -> Feature -> Task -> ADR -> Test Plan)
- [ ] Self-review completed and ready for review
