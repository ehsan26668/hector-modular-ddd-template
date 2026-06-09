# ADR 0034: Dead Letter and Poison Message Handling

## Status

Proposed

## Context

The outbox processor retries failed event publications according to configured retry limits.

This improves resilience for transient failures such as:

- temporary database issues
- short network interruptions
- broker availability problems

However, some failures are not transient.

Examples include:

- invalid payloads
- deserialization failures
- unsupported event types
- permanent contract mismatches
- handler logic errors

Such messages may fail repeatedly and become poison messages.

Without a defined dead-letter strategy, poison messages can cause repeated processing attempts, noisy logs, operational confusion, and hidden data loss.

The system needs an explicit policy for identifying, isolating, and diagnosing permanently failing messages.

## Decision

The system will adopt a **dead-letter and poison message handling strategy** for outbox processing.

When an outbox message reaches the configured maximum retry count and still cannot be processed successfully, it will be treated as a poison message.

Poison messages will no longer participate in normal outbox processing.

### Poison Message Criteria

An outbox message is considered poison when:

- `RetryCount` reaches `MaxRetryCount`
- the message still fails during processing

### Dead-Letter Handling

Instead of remaining eligible for normal processing, poison messages must be marked explicitly as failed.

Recommended fields include:

- `FailedOn`
- `FailureReason`
- `IsPoisoned`

Example:

    public DateTime? FailedOn { get; set; }
    public string? FailureReason { get; set; }
    public bool IsPoisoned { get; set; }

Once marked as poison:

- the message must be excluded from normal outbox queries
- the failure reason must be preserved
- the message must remain available for diagnostics and manual recovery

### Operational Visibility

Poison messages must be clearly visible to operators.

This may include:

- structured error logging
- database queries for failed messages
- dashboards or monitoring alerts
- administrative tooling for replay or inspection

### Manual Recovery

Poison messages should not be automatically deleted.

Instead, they should remain recoverable so operators can:

- inspect the payload
- correct underlying issues
- requeue the message manually if appropriate

Automatic replay of poison messages is not recommended without operator intent.

### Relationship to Delivery Guarantees

This strategy complements the at-least-once delivery model.

Transient failures continue to use retry behavior.

Permanent failures transition to dead-letter state instead of retrying indefinitely.

## Consequences

Positive:

- Prevents endless retry loops for permanently failing messages.
- Improves operational visibility into broken events.
- Preserves failed messages for diagnosis and recovery.
- Makes outbox processing behavior more production-ready.

Negative:

- Requires additional schema fields or dead-letter storage.
- Introduces operational processes for reviewing failed messages.
- Some failures will require manual intervention before recovery.
