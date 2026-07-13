# Test Plan: ADR-0058 Adopt ADR-Driven Backlog and Delivery Governance

## Status

Accepted

## Context

This test plan validates the **GitHub-native governance and enforcement mechanisms** described in [ADR-0058](/docs/adr/0058-adopt-adr-driven-backlog-and-delivery-governance.md).  
The goal is to ensure that the "Separation of Concerns" between governance definition (docs) and governance enforcement (GitHub) is operationally sound. This is critical to ensure that architectural decisions are machine-traceable to backlog items, Features, and Pull Requests without polluting the application codebase.

## Test Strategy

Define the layers of testing to be used:

- **Governance Workflow Validation:**
  - Focus on GitHub Actions, YAML schema, and automated enforcement of required fields/labels.
  - Target: `.github/workflows/` and `.github/governance/`

- **Template Conformance Testing:**
  - Focus on Issue and PR templates ensuring they capture mandatory metadata (ADR refs, linkage).
  - Target: `.github/ISSUE_TEMPLATE/` and `.github/pull_request_template.md`

- **Repository Structure Guard (BAG):**
  - Focus on ensuring `backlog.md` and `decision-log.md` remain consistent with the repository state.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

- **Included:**
  - ADR traceability and numbering rules.
  - Automated labeling taxonomy (e.g., `area/*`, `status/*`).
  - GitHub Issue Template enforcement (Feature, Task, ADR).
  - PR Template traceability (linkage to Issues/ADRs).
  - Backlog Architecture Guard (BAG) validation logic.

- **Excluded:**
  - External PM tools (Jira, Trello).
  - Sprint-level manual planning.
  - Advanced GitHub Project V2 UI custom fields.

---

## 2. Test Cases (Governance / Automation)

### TC-01: Should_BlockMerge_When_PRMissingTraceabilityLink

**Scenario:** A developer attempts to merge a PR without referencing a governed Issue or ADR.

**Arrange:**

- Create a PR branch.
- Ensure the PR body is empty or lacks mandatory "Ref: #" or "Closes #" links.

**Act:**

- Trigger the `governance-pr-check` workflow.

**Assert:**

- Verify workflow fails and blocks the merge button.

### TC-02: Should_AssignGovernanceLabels_When_IssueCreatedFromTemplate

**Scenario:** A new Feature is proposed using the standardized Issue Template.

**Arrange:**

- Select "Feature Request" template in GitHub UI.
- Fill in mandatory "Architecture Ref" field.

**Act:**

- Submit the Issue.

**Assert:**

- Verify `governance/feature` and `status/proposed` labels are automatically applied.

### TC-03: Should_FailBAGCheck_When_BacklogReferencesNonExistentADR

**Scenario:** `backlog.md` is updated to include a feature referencing an ADR that doesn't exist.

**Arrange:**

- Edit `docs/backlog/backlog.md` adding a line with `[ADR-9999]`.

**Act:**

- Run the BAG (Backlog Architecture Guard) automated test.

**Assert:**

- Verify a descriptive failure message: "ADR-9999 not found in /docs/adr/".

---

## 3. Non-Functional Validation Points

### 3.1 Security & Sanitization

- Verify that workflow secrets are masked.
- Ensure `CODEOWNERS` protects the `.github/` folder from unauthorized changes.

### 3.2 Observability & Traceability

- Verify that the path from ADR -> Feature Issue -> Pull Request -> Code is fully auditable through GitHub's linked-item graph.

### 3.3 Contract Stability

- Verify that the label taxonomy remains stable and follows the `docs/backlog/backlog.md` definitions.

---

## 4. Test Data

- **Inputs:**
  - Sample `backlog.md` entries.
  - Compliant vs. Malformed ADR files.
  - PR payloads with and without required metadata.

- **Expected Outputs:**
  - Standardized error comments on PRs for failed governance.
  - Automated project board movements on status label changes.

---

## 5. TDD Execution Plan

1. **RED**
   - Implement a failing BAG test in `Hector.ArchitectureTests` that checks for mandatory ADR sections.
   - Run CI to confirm failure.

2. **GREEN**
   - Update `adr-template.md` and existing ADRs to comply.
   - Adjust GitHub Action to enforce the check.

3. **REFACTOR**
   - Optimize workflow triggers to run only on relevant path changes (e.g., `docs/**` or `.github/**`).

---

## 6. Exit Criteria

- [x] ADR-0058 is linked in the `decision-log.md`.
- [ ] Issue templates for ADR, Feature, and Task are operational in `.github/`.
- [ ] PR template includes mandatory "Traceability" section.
- [ ] BAG (Backlog Architecture Guard) basic automation is running in CI.

---

## 7. Proposed Test File Layout

```text
.github/
 ├── governance/
 │   ├── area-path-rules.yml        # Logic for path-based ownership
 │   └── labels.yml                 # Taxonomy definition
 ├── ISSUE_TEMPLATE/
 │   ├── architecture_change.md     # ADR Proposal template
 │   ├── bug_report.md              # Standard bug report
 │   ├── epic.md                    # Epic/Initiative template
 │   ├── feature_item.md            # Feature template
 │   ├── task_item.md               # Task template
 │   └── config.yml                 # Template chooser configuration
 ├── workflows/
 │   ├── architecture-tests.yml     # Architecture Guard execution
 │   ├── build.yml                  # CI Build & Compile
 │   ├── docs-validation.yml        # Documentation & Link integrity
 │   ├── labeler.yml                # Path-to-label automation logic
 │   ├── pr-labels-from-body.yml    # Metadata-to-label automation
 │   ├── pr-validation.yml          # Traceability & Governance enforcement
 │   └── tests.yml                  # Main CI Pipeline (Unit/Integration)
 ├── CODEOWNERS                      # Path-based approval rules
 ├── labeler.yml                    # (Redundant - Potential Cleanup Candidate)
 └── pull_request_template.md       # PR structure with Ref sections
```

## Summary

This test plan ensures that ADR-0058 is validated as a **living governance system** where GitHub acts as the runtime enforcement agent.
