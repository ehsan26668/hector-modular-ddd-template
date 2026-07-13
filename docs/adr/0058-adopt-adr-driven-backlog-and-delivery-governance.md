# ADR 0058: Adopt ADR-Driven Backlog and Delivery Governance

## Status

Accepted

## Context

The project requires a lightweight but enforceable governance model for
backlog management, delivery discipline, and traceability from
architecture decisions to implementation work.

Earlier documentation-centered approaches improved visibility, but they
did not by themselves guarantee process conformance. Manual discipline
alone is not sufficient for sustaining consistent delivery practices
across ADRs, Features, Tasks, Pull Requests, and release evidence.

The repository already contains the foundations of a GitHub-native
governance model under `.github/`, including issue templates, pull
request templates, labels, and workflows. This creates an opportunity to
treat GitHub not merely as a collaboration surface, but as the runtime
environment where governance rules are operationally enforced.

The desired doctrine is:

1. Governance belongs to GitHub, not application code.
2. Documentation defines the model; GitHub enforces the model.
3. Project code should remain clean from process orchestration concerns.
4. Traceability should be machine-checkable wherever possible.
5. Manual discipline is fallback, not primary enforcement.

## Decision

We adopt a GitHub-native governance architecture for backlog and delivery
management.

Governance will be defined in repository documentation, but enforcement
will primarily occur through GitHub artifacts and automation in
`.github/`.

This ADR establishes four cooperating governance layers:

### 1. Policy Layer

The Policy Layer defines the governance model and intent through
documentation artifacts such as:

- ADRs
- standards
- templates
- decision registry

This layer explains what must be true.

### 2. Planning Layer

The Planning Layer defines the human-readable execution structure of
planned work through artifacts such as:

- `docs/backlog/backlog.md`
- ADR-to-Feature-to-Task hierarchy
- label taxonomy
- backlog conventions

This layer explains how work is organized.

### 3. Enforcement Layer

The Enforcement Layer operationalizes governance through GitHub-native
mechanisms such as:

- issue templates
- pull request templates
- labels
- CODEOWNERS
- GitHub Actions workflows
- validation rules
- merge-time checks

This layer ensures rules are checked automatically.

### 4. Evidence Layer

The Evidence Layer captures auditable proof that governance has been
followed, including:

- linked Issues
- linked Pull Requests
- linked ADRs
- linked Test Plans
- workflow results
- merge history

This layer proves what happened.

## Governance Reference and Enforcement Model

To avoid coupling governance concerns into the application codebase, the
repository will separate documentation from enforcement responsibility.

- `docs/decisions/decision-log.md` is the registry of architectural
  decisions and their lifecycle state.
- `docs/backlog/backlog.md` is the canonical governance reference for
  backlog structure, hierarchy, and planning intent.
- GitHub Issues, Pull Requests, labels, templates, Projects, and Actions
  are the operational governance runtime where process rules are enforced.
- `.github/` is the primary source of executable governance behavior.

Accordingly, this ADR does not treat `backlog.md` as the sole operational
source of truth for delivery state. Instead:

- documentation is the source of governance definition,
- GitHub is the source of runtime governance enforcement,
- workflow evidence is the source of compliance visibility.

## Backlog Architecture Guard (BAG)

The Backlog Architecture Guard (BAG) is introduced as the enforcement
concept that protects backlog structure, traceability, and delivery
discipline.

BAG will be implemented in two layers:

### Primary Layer: GitHub Workflow Enforcement

The first and preferred BAG implementation resides in GitHub automation,
including checks such as:

- required issue template conformance
- required PR template conformance
- label presence and taxonomy validation
- linkage validation between Task, Feature, and ADR
- completion-state evidence checks
- documentation cross-reference validation

This layer is the default enforcement mechanism.

### Secondary Layer: Repository Test Support

If governance logic grows in complexity, selective rule validation may be
added as a secondary layer in `Hector.ArchitectureTests` or related DSL
infrastructure.

This layer is optional and must not become the primary home of delivery
governance. It exists only to support repository-level validation where
test execution provides clear value.

## Separation of Concerns

The repository maintains distinct artifacts with complementary but
different responsibilities:

| Artifact | Responsibility | Nature |
| --- | --- | --- |
| `docs/decisions/decision-log.md` | Registry of architecture decisions and status | Historical / Registry |
| `docs/backlog/backlog.md` | Canonical planning map of governed work | Structural / Planning |
| `.github/` templates, labels, workflows, actions | Execution and enforcement of governance rules | Runtime / Enforcement |
| linked Issues, PRs, Test Plans, workflow runs | Evidence of compliance and delivery traceability | Audit / Evidence |

This separation keeps the application codebase free from process
orchestration concerns while still enabling strict, automatable
governance.

## Consequences

### Positive

- Governance becomes executable rather than purely documented.
- Delivery discipline is enforced before merge, not only reviewed after.
- Traceability from ADR to implementation becomes machine-checkable.
- The application codebase remains clean from workflow-process concerns.
- Auditability improves through workflow evidence, labels, and linkage.
- Governance can evolve incrementally through `.github/` without forcing
  product code changes.

### Negative

- Governance logic becomes distributed across documentation and GitHub
  configuration rather than living in a single file.
- Initial setup and maintenance effort increases for templates, labels,
  and workflows.
- Poorly designed automation can create friction if rules are too rigid
  or noisy.
- Teams must keep documentation and GitHub enforcement aligned to avoid
  drift.

### Neutral / Trade-off

- `backlog.md` remains important, but primarily as a canonical reference
  map rather than the sole runtime state holder.
- Some governance checks may temporarily rely on manual review until
  automation is fully implemented.

## Compliance and Enforcement

Compliance with this ADR will be enforced primarily through GitHub-native
mechanisms:

1. Issue templates that constrain the creation of ADR-, Epic-, Feature-,
   and Task-aligned work items.
2. Pull request templates that require explicit linkage to governed work
   and related ADRs.
3. Labels and taxonomy rules that support classification, ownership, and
   workflow policy checks.
4. GitHub Actions workflows that validate documentation, metadata,
   references, and required governance signals.
5. CODEOWNERS and review policies that ensure the right governance
   stakeholders are included.
6. Manual review only as a bootstrap fallback where automation is not
   yet available.

## Implementation Notes

The expected implementation surface for this ADR is primarily under
`.github/`, including but not limited to:

- `.github/ISSUE_TEMPLATE/`
- `.github/pull_request_template.md`
- `.github/labels.yml`
- `.github/governance/`
- `.github/workflows/`
- `CODEOWNERS`

Supporting documentation is expected under:

- `docs/decisions/decision-log.md`
- `docs/backlog/backlog.md`
- `docs/tests/`
- relevant ADR documents

## Validation Strategy

Validation of this ADR should focus on whether governance is actually
enforced through GitHub workflows and repository conventions, not merely
documented.

Representative validation scenarios include:

- a Feature cannot be created without required metadata
- a Task must link to a parent Feature or ADR
- a Pull Request must reference governed work
- a completed work item must include delivery evidence
- documentation links must remain internally consistent
- governance labels must match approved taxonomy

## Related Artifacts

- `docs/decisions/decision-log.md`
- `docs/backlog/backlog.md`
- `.github/ISSUE_TEMPLATE/`
- `.github/pull_request_template.md`
- `.github/workflows/`
- `.github/governance/`
- `CODEOWNERS`

## Decision Summary

Backlog and delivery governance will be defined by documentation, but
enforced primarily through GitHub-native automation and repository
governance artifacts.

GitHub is the governance runtime.
