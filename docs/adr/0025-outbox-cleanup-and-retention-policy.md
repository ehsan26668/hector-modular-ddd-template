# ADR 0025: Introduce Outbox Cleanup and Retention Policy

## Status

Accepted

## Context

ADR‑0021 introduced the Transactional Outbox pattern to reliably persist domain events alongside application state changes.

ADR‑0022 introduced the Outbox Processor responsible for publishing events stored in the OutboxMessages table.

Over time, the Outbox table continuously accumulates records as new events are produced and processed. In production systems with high event throughput, this table can grow rapidly and eventually contain millions of rows.

Unbounded growth of the Outbox table may lead to several problems:

- increased database storage usage
- slower index scans
- degraded query performance during outbox polling
- longer backup and restore times
- operational complexity when investigating data

However, completely deleting messages immediately after processing may not be desirable because processed messages can provide valuable diagnostic and auditing information.

A retention strategy is therefore required to balance operational visibility with database performance.

## Decision

We will introduce a retention and cleanup policy for processed Outbox messages.

Processed messages will be retained for a configurable time window before being removed from the database.

A background cleanup job will periodically delete old processed messages according to the retention policy.

The cleanup process will follow these rules:

1. Only messages with ProcessedOn not null are eligible for cleanup.
2. Messages are retained for a configurable retention period (for example 7–30 days).
3. Messages older than the retention threshold will be deleted in batches.
4. Cleanup operations must avoid large transactions and use batched deletes.

Example cleanup query:

```text
DELETE FROM OutboxMessages

WHERE ProcessedOn < (current_time - retention_period)

LIMIT batch_size
```

The cleanup job may run periodically (for example every hour or every day) depending on system throughput.

Retention duration and cleanup batch size will be configurable through application settings.

## Consequences

Positive:

- prevents uncontrolled growth of the OutboxMessages table
- improves long‑term database performance
- reduces storage costs
- keeps the outbox polling query efficient
- maintains operational visibility for recently processed messages

Negative:

- historical event records are eventually removed
- requires an additional background job
- cleanup operations must be carefully tuned to avoid database contention
