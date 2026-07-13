# Backlog Governance Standard

## Purpose

The purpose of this standard is to establish a rigorous, machine-checkable, and human-readable governance model for the repository's backlog and delivery lifecycle. It operationalizes the principles defined in [ADR-0058](https://github.com/ehsan26668/hector-modular-ddd-template/blob/main/docs/adr/0058-adopt-adr-driven-backlog-and-delivery-governance.md), ensuring that every implementation detail is traceable back to an architectural decision or a verified requirement.

## Scope

This standard applies to all work items (Epics, Features, Tasks), Pull Requests, and documentation artifacts within the `hector-modular-ddd-template` repository. It governs how traceability is recorded, how links are formatted in GitHub Issues, and the minimum evidence required for a work item to be considered "Done."

## Core Rules

1. **ADR-First:** No implementation with architectural impact (as defined in ADR-0058) shall start without a corresponding Architecture Decision Record in the `Proposed` or `Accepted` state.
2. **Traceability-by-Design:** Every Pull Request must be traceable to a Task, which must be traceable to a Feature. Orphans are not allowed.
3. **Evidence-Driven:** A work item is not complete until its corresponding validation evidence (Tests, Test Plans, or CI Green Signals) is linked and verified.
4. **GitHub-Native Enforcement:** Governance must be enforced through GitHub-native mechanisms (Templates, Actions, Labels, and CODEOWNERS) to minimize manual oversight.
5. **No Broken Windows:** Broken links, missing parent references, or non-compliant issue bodies are considered governance violations and must be remediated.

## Issue Linking Policy

To preserve human-readable traceability and ensure links remain clickable and valid in GitHub Issues, all repository document references inside Issue bodies must use one of the following formats:

- **Full GitHub blob links**  
  Example: `https://github.com/ehsan26668/hector-modular-ddd-template/blob/main/docs/adr/0058-example.md`
- **Root-relative repository links**  
  Example: `/ehsan26668/hector-modular-ddd-template/blob/main/docs/adr/0058-example.md`

Plain relative Markdown links such as `docs/adr/0058-example.md` or `../standards/file.md` must not be used inside GitHub Issue bodies, because GitHub resolves them relative to the Issue URL rather than the repository root, which can produce broken links.

### Required behavior

- All ADR, Test Plan, Standard, and related repository document references in Epic, Feature, and Task issues must be clickable from the rendered GitHub Issue.
- Link targets must resolve correctly without requiring manual path editing by reviewers.
- When traceability references are included for machine or human validation, clickable links are preferred over plain text paths.

### Allowed exception

If a path must be preserved for portability or templating reasons, it may be shown as inline code or plain text in addition to the clickable link, but it must not replace the clickable repository-valid link in rendered GitHub Issues.

## Pull Request Linking Requirements

Every Pull Request must clearly identify:

- the governed work item being implemented,
- the parent planning context,
- the architectural decision context when applicable,
- the validation artifact or Test Plan when applicable.

At minimum:

- **PR -> Task** is required
- **PR -> Feature** is required
- **PR -> Epic** is recommended when applicable
- **PR -> ADR** is required for architecture-relevant changes
- **PR -> Test Plan** is required when an ADR-linked validation artifact exists

## Traceability Rules

The minimum traceability chain must remain explicit and reviewable:
`Epic -> Feature -> Task -> ADR -> Test Plan -> Tests -> PR -> CI`

The following relationships apply:

- An Epic represents a top-level initiative.
- A Feature must link to its parent Epic when one exists.
- A Task must link to exactly one parent Feature.
- An ADR must be linked when the work has architectural impact.
- A Test Plan must be linked to the corresponding ADR.
- A Pull Request must link to the implemented Task, related Feature, and any applicable ADR, Test Plan, or governing standard.
- Related standards and policy documents must be referenced when they materially govern the work item or its review.

## Exceptions and Temporary Gaps

Any temporary traceability gap must be explicitly documented in the relevant Issue or Pull Request.
Examples include:

- ADR pending but implementation preparation started
- Test Plan follow-up created but not yet merged
- Standard update tracked separately
- Parent work item not yet created at the moment of drafting

Such gaps must be accompanied by a follow-up work item and must be resolved before final completion or merge, unless an explicit reviewer-approved exception is recorded.

## Definition of Compliance

A backlog item is governance-compliant only when:

- required parent links are present,
- required ADR / Test Plan references are present when applicable,
- links are rendered as valid clickable repository links inside GitHub,
- related documentation references are reviewable,
- Pull Request traceability is complete before merge.
