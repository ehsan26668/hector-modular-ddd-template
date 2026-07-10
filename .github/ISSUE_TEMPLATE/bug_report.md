---
name: Bug Report
about: Report a defect in the template, framework, or modules
title: "[BUG] "
labels: ["bug"]
assignees: []
---

## Problem

Provide a clear and concise description of the defect.

## Affected Area

- **Module:**
  - [ ] Projects
  - [ ] Framework (Building Blocks)
  - [ ] Host
  - [ ] Cross-module
- **Layer:**
  - [ ] Domain
  - [ ] Application
  - [ ] Infrastructure / Persistence
  - [ ] Web / Host
  - [ ] Architecture Tests / Guard Rules
  - [ ] CI/CD / Template Packaging

## Environment Details

- **Template Version / Commit Hash:**
- **.NET SDK Version:**
- **OS / IDE:**

## Reproduction Steps

1.
2.
3.

## Expected Behavior

What should have happened according to existing specifications, ADRs, or standards?

## Actual Behavior

What actually happened? Include stack trace, logs, or failing test output if available.

Logs / stack trace / failing output:

```text
// Paste logs, stack traces, or failing test output here
```

## Traceability & Root Cause Analysis

- **Suspected Root Cause:**
- **Related ADR / Decision Log Entry:** <!-- If the bug stems from a design decision or ADR violation -->
- **Existing Test Plan Reference:** <!-- Link to the test plan that failed to catch this -->

## Proposed Test Coverage & Fixing Checklist

- [ ] **Test Types Needed:**
  - [ ] Unit Test (for domain/application logic)
  - [ ] Integration Test (for persistence/infrastructure)
  - [ ] Architecture Test (for boundary/naming violations)
  - [ ] Regression Test (to prevent recurrence)
- [ ] Fix implemented
- [ ] Regression risk assessed (Describe what else could be affected by this fix)
