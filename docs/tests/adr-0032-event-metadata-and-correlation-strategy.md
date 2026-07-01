# Test Plan: ADR-0032 Event Metadata and Correlation Strategy

## Status

Accepted

## Context

This test plan validates the **Event Metadata and Correlation Strategy** defined in [ADR-0032](/docs/adr/0032-event-metadata-and-correlation-strategy.md).

The system relies on domain events and integration events persisted through the Transactional Outbox pattern. As event flows grow across modules, standardized metadata (`EventId`, `CorrelationId`, `CausationId`, `OccurredOn`, `Producer`, `Version`) is essential for end-to-end tracing, debugging, and observability.

This plan validates that:

- Every event carries the full set of required metadata fields.
- `CorrelationId` propagates correctly from the incoming HTTP request through commands to domain events and integration events.
- `CausationId` correctly references the triggering command or parent event.
- `EventId` is uniquely generated for each event instance.
- `OccurredOn` is set at event creation time.
- `Producer` identifies the originating module.
- `Version` reflects the event contract version from `OutboxEventAttribute`.
- The `OutboxMessage` stores metadata fields separately for querying and diagnostics.
- The `CorrelationMiddleware` extracts/propagates correlation headers at the web boundary.
- The `CorrelationContextAccessor` makes the current `CorrelationContext` available throughout the request pipeline.

## Test Strategy

- **Unit Tests:**
  - Validate `CorrelationContext` initialization and propagation.
  - Validate `CorrelationContextAccessor` get/set behavior in async context.
  - Validate `CorrelationMiddleware` header extraction and generation.
  - Validate `DefaultOutboxMessageFactory` metadata population (`EventId`, `CorrelationId`, `CausationId`, `OccurredOn`, `Producer`, `Version`).
  - Validate `OutboxEventMetadata` structure and field completeness.
  - Validate `DomainEventBase` metadata assignment (`EventId`, `OccurredOn`).
  - Target Project: `tests/UnitTests/Hector.BuildingBlocks.Application.UnitTests`, `tests/UnitTests/Hector.BuildingBlocks.Persistence.UnitTests`, `tests/UnitTests/Hector.BuildingBlocks.Domain.UnitTests`, `tests/UnitTests/Hector.BuildingBlocks.Web.UnitTests`

- **Integration Tests:**
  - Validate end-to-end correlation propagation: HTTP Request → Command → Domain Event → Integration Event → Outbox Message.
  - Validate that the `OutboxMessage` row in the database contains all metadata columns correctly populated.
  - Validate outbox message metadata is queryable.
  - Target Project: `tests/IntegrationTests/Hector.Modules.Projects.IntegrationTests`, `tests/IntegrationTests/Hector.BuildingBlocks.Persistence.IntegrationTests`

- **Architecture Tests:**
  - Enforce that all integration events and domain events expose required metadata fields.
  - Enforce that `OutboxMessage` contains all metadata columns.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`

---

## 1. Scope

- **Included:**
  - Standardized metadata field presence on events (`EventId`, `CorrelationId`, `CausationId`, `OccurredOn`, `Producer`, `Version`).
  - `CorrelationMiddleware` header extraction and generation of `CorrelationId`.
  - `CorrelationContextAccessor` propagation through async pipeline.
  - `DefaultOutboxMessageFactory` metadata population from `CorrelationContext` and event attributes.
  - `OutboxMessage` separate metadata columns for diagnostics.
  - `DomainEventBase` base metadata assignment.
  - End-to-end causal chain reconstruction: Request → Command → DomainEvent → IntegrationEvent.
  - `Version` field sourced from `OutboxEventAttribute`.

- **Excluded:**
  - Integration with external observability tools (OpenTelemetry, Jaeger, etc.).
  - Message broker header propagation (transport-specific).
  - Outbox cleanup and retention policy behavior.
  - Performance benchmarks for metadata overhead.

---

## 2. Test Cases

### TC-01: Should_ExtractOrGenerateCorrelationId_WhenCorrelationMiddlewareProcessesRequest

**Scenario:** The `CorrelationMiddleware` must extract `CorrelationId` from the incoming HTTP header if present, or generate a new one if absent.

**Arrange:**

- Build a `DefaultHttpContext` with no correlation header.
- Build a separate `DefaultHttpContext` with a pre-existing correlation header value.

**Act:**

- Invoke `CorrelationMiddleware` for both contexts.

**Assert:**

- When header is absent: a new `Guid` is generated and set in `CorrelationContextAccessor`.
- When header is present: the existing value is used and propagated.
- The correlation header name matches `CorrelationHeaderNames.CorrelationId`.

---

### TC-02: Should_PropagateCorrelationContext_AcrossAsyncAwaitBoundary

**Scenario:** The `CorrelationContextAccessor` must preserve the `CorrelationContext` across `await` points within the same logical request.

**Arrange:**

- Create a `CorrelationContextAccessor` with an `AsyncLocal<CorrelationContext?>` backing field.
- Set a `CorrelationContext` with a known `CorrelationId`.

**Act:**

- Await a `Task.Delay` and then read the `CorrelationContext` again.

**Assert:**

- The `CorrelationId` remains consistent before and after the await.
- No cross-request leakage occurs between concurrent async operations.

---

### TC-03: Should_AssignEventIdAndOccurredOn_WhenDomainEventBaseIsCreated

**Scenario:** Every domain event inheriting `DomainEventBase` must have a unique `EventId` and a valid `OccurredOn` timestamp at construction time.

**Arrange:**

- Define a test domain event inheriting `DomainEventBase`.

**Act:**

- Instantiate the test event.

**Assert:**

- `EventId` is a non-empty `Guid`.
- `OccurredOn` is set to approximately `DateTime.UtcNow` (within a small tolerance).
- Two separate instances have different `EventId` values.

---

### TC-04: Should_PopulateAllMetadataFields_WhenDefaultOutboxMessageFactoryCreatesMessage

**Scenario:** The `DefaultOutboxMessageFactory` must populate all six metadata fields (`EventId`, `CorrelationId`, `CausationId`, `OccurredOn`, `Producer`, `Version`) when creating an `OutboxMessage` from an integration event.

**Arrange:**

- Create a sample `IIntegrationEvent` implementation with `[OutboxEvent("test.event", 2)]` attribute.
- Set up a `CorrelationContextAccessor` with a known `CorrelationId` and `CausationId`.
- Substitute `IOutboxEventSerializer` to return a valid serialized payload.

**Act:**

- Call `DefaultOutboxMessageFactory.CreateAsync(event, cancellationToken)`.

**Assert:**

- Resulting `OutboxMessage` has:
  - `EventId` = a non-empty `Guid` (unique per event).
  - `CorrelationId` = value from `CorrelationContext`.
  - `CausationId` = value from `CorrelationContext` (or `null` if not set).
  - `OccurredOn` = valid timestamp.
  - `Producer` = the module/assembly name of the event.
  - `Version` = `2` (from `OutboxEventAttribute`).

---

### TC-05: Should_PropagateCorrelationId_FromCommandToDomainEventToIntegrationEvent

**Scenario:** When a command handler processes a command and produces a domain event that is then mapped to an integration event, the `CorrelationId` must remain consistent across the entire chain.

**Arrange:**

- Set `CorrelationContextAccessor` with `CorrelationId = X`.
- Execute `CreateProjectCommand` with `CorrelationId = X`.

**Act:**

- The command handler creates a `Project` aggregate which raises `ProjectCreatedDomainEvent`.
- The `ProjectCreatedDomainEventHandler` maps to `ProjectCreatedIntegrationEvent`.
- The `OutboxIntegrationEventBus` publishes the integration event via outbox.

**Assert:**

- `ProjectCreatedDomainEvent.CorrelationId` == `X`.
- `ProjectCreatedIntegrationEvent` (or its outbox message) `CorrelationId` == `X`.
- `CausationId` of the integration event == `EventId` of the domain event.
- The entire causal chain can be reconstructed from `CorrelationId`.

---

### TC-06: Should_SetCausationId_ToTriggeringCommandOrEventId

**Scenario:** `CausationId` must reference the direct cause of the event — the triggering command's ID or the parent event's `EventId`.

**Arrange:**

- Create a command with a known `CommandId`.
- Set `CorrelationContext` with `CausationId = CommandId`.

**Act:**

- Process the command which raises a domain event.

**Assert:**

- The domain event's `CausationId` (via `CorrelationContext`) == `CommandId`.
- When the domain event is mapped to an integration event, the integration event's `CausationId` == domain event's `EventId`.

---

### TC-07: Should_StoreMetadataInSeparateOutboxMessageColumns_ForDiagnostics

**Scenario:** The `OutboxMessage` entity must persist metadata fields as separate database columns (not only embedded in the serialized payload) to enable efficient querying.

**Arrange:**

- Use `WebApplicationFactory` / `TestApplicationFactory` to spin up the real database.
- Execute a command that produces an outbox message.

**Act:**

- Query the `OutboxMessage` table directly.

**Assert:**

- `OutboxMessage` row contains non-null values for:
  - `EventId` (unique `Guid`)
  - `CorrelationId` (`Guid`)
  - `CausationId` (`Guid?`)
  - `OccurredOn` (`DateTime`)
  - `Producer` (`string`)
  - `Version` (`int`)
- These columns are separate from the serialized `Payload` column.

---

### TC-08: Should_SetProducer_ToModuleName_WhenEventIsPublished

**Scenario:** The `Producer` metadata field must identify the module that produced the event.

**Arrange:**

- Publish an integration event from the `Projects` module.
- Create the outbox message via `DefaultOutboxMessageFactory`.

**Act:**

- Inspect the `Producer` field of the resulting `OutboxMessage`.

**Assert:**

- `Producer` contains the module name (e.g., `"Projects"` or the assembly-derived identifier).
- `Producer` is not empty or null.

---

### TC-09: Should_SetVersion_FromOutboxEventAttribute_WhenEventHasAttribute

**Scenario:** The `Version` metadata field must reflect the version declared in the `OutboxEventAttribute` on the integration event contract.

**Arrange:**

- Define an integration event with `[OutboxEvent("projects.project-created", 1)]`.

**Act:**

- Create an outbox message for this event.

**Assert:**

- `OutboxMessage.Version` == `1`.
- If the attribute declares version `3`, `OutboxMessage.Version` == `3`.

---

### TC-10: Should_GenerateUniqueEventId_ForEachEventInstance

**Scenario:** Each event instance must have a globally unique `EventId`, even when the same event type is produced multiple times within the same correlation.

**Arrange:**

- Produce `N` instances of the same integration event type within a single correlation context.

**Act:**

- Collect all `EventId` values from the resulting outbox messages.

**Assert:**

- All `N` `EventId` values are distinct `Guid`s.
- No collision occurs.

---

### TC-11: Should_PropagateCorrelationId_WhenNoCorrelationHeaderPresent

**Scenario:** If no correlation header arrives with the HTTP request, the middleware generates a new `CorrelationId` and it must propagate to all downstream events.

**Arrange:**

- Send an HTTP request without any correlation header.

**Act:**

- Process through the full pipeline: middleware → command → domain event → integration event → outbox.

**Assert:**

- A new `CorrelationId` is generated by `CorrelationMiddleware`.
- All events in the chain share the same generated `CorrelationId`.
- The `OutboxMessage.CorrelationId` matches the generated value.

---

### TC-12: Should_ReconstructCausalChain_UsingCorrelationAndCausationIds

**Scenario:** Given a set of outbox messages from a single request, the full causal chain must be reconstructable using `CorrelationId` and `CausationId`.

**Arrange:**

- Execute a command that produces: 1 domain event → 1 integration event → 1 outbox message.
- Query all outbox messages with the same `CorrelationId`.

**Act:**

- Order messages by `OccurredOn` and link each `CausationId` to the preceding event's `EventId`.

**Assert:**

- The chain is complete with no orphaned `CausationId` references.
- The root of the chain has `CausationId == null` or references the original command.

---

### TC-13: Should_AllIntegrationEventsExposeRequiredMetadata_ArchitectureGuard

**Scenario:** All `IIntegrationEvent` implementations must carry or be associated with the full set of metadata fields.

**Arrange:**

- Scan all assemblies for types implementing `IIntegrationEvent`.

**Act:**

- Verify via `NetArchTest.Rules` that each integration event is decorated with `[OutboxEvent]` attribute (which provides `Version` and canonical name).

**Assert:**

- Every integration event has `[OutboxEvent]` attribute.
- The `OutboxMessage` entity exposes all six metadata properties.

---

### TC-14: Should_OutboxMessageContainAllMetadataColumns_ArchitectureGuard

**Scenario:** The `OutboxMessage` entity must expose all six metadata fields as public properties.

**Arrange:**

- Reflect on `OutboxMessage` type.

**Act:**

- Check for presence of properties: `EventId`, `CorrelationId`, `CausationId`, `OccurredOn`, `Producer`, `Version`.

**Assert:**

- All six properties exist on `OutboxMessage`.
- All are publicly accessible.

---

## 3. Non-Functional Validation Points

### 3.1 Observability & Traceability

- `CorrelationId` must be consistent across the entire request → event chain for distributed tracing.
- `CausationId` must enable parent-child event relationship reconstruction.
- Metadata must be queryable from the `OutboxMessage` table without deserializing the payload.
- `CorrelationMiddleware` must add the correlation header to the HTTP response for client-side tracing.

### 3.2 Stability

- `CorrelationContextAccessor` must use `AsyncLocal<T>` to prevent cross-request leakage in concurrent scenarios.
- `EventId` generation must be collision-free (use `Guid.NewGuid()` or `Guid.CreateVersion7()`).
- Metadata propagation must not fail silently; missing correlation context should result in a generated `CorrelationId` with a clear log warning.

### 3.3 Performance

- Metadata fields add fixed overhead per event (6 fields); this must not exceed acceptable message size thresholds.
- `CorrelationContextAccessor` must not introduce measurable latency in the hot path.

### 3.4 Security

- `CorrelationId` and `CausationId` must not leak sensitive internal identifiers to external consumers unless explicitly part of the contract.
- `Producer` field must not expose internal assembly paths or infrastructure details beyond the module name.

---

## 4. Test Data

- **Inputs:**
  - HTTP requests with and without `X-Correlation-ID` header.
  - Commands with known `CommandId` and `CorrelationId`.
  - Integration events decorated with `[OutboxEvent(name, version)]`.
  - `CorrelationContext` instances with varying `CausationId` (null, command ID, event ID).
  - Concurrent async operations to test `AsyncLocal` isolation.

- **Expected Outputs:**
  - `OutboxMessage` rows with all six metadata fields correctly populated.
  - Consistent `CorrelationId` across the full chain.
  - Correct `CausationId` linking between parent and child events.
  - Unique `EventId` per event instance.
  - `Version` matching the `OutboxEventAttribute` declaration.

---

## 5. TDD Execution Plan

1. **RED**
   - Write a test that publishes an integration event and asserts `OutboxMessage.CorrelationId` matches the request's correlation ID.
   - Observe failure if `CorrelationContext` is not propagated to `DefaultOutboxMessageFactory`.
   - Write a test that asserts `CausationId` links integration event to domain event's `EventId`.
   - Observe failure if causal linking is not implemented.

2. **GREEN**
   - Implement `CorrelationContextAccessor` using `AsyncLocal<CorrelationContext?>`.
   - Implement `CorrelationMiddleware` to extract/generate `CorrelationId` and set `CorrelationContext`.
   - Ensure `DefaultOutboxMessageFactory` reads from `CorrelationContextAccessor` and `OutboxEventAttribute`.
   - Ensure `DomainEventBase` assigns `EventId` and `OccurredOn` at construction.
   - Ensure the application layer event handler sets `CausationId` when mapping domain events to integration events.

3. **REFACTOR**
   - Extract reusable test fixtures for correlation context setup.
   - Create a shared `CorrelationTestHelper` for asserting chain consistency.
   - Ensure architecture tests are DRY and extensible for new modules.

---

## 6. Exit Criteria

- [ ] `CorrelationMiddleware` extracts or generates `CorrelationId` correctly.
- [ ] `CorrelationContextAccessor` propagates context across async boundaries without leakage.
- [ ] `DomainEventBase` assigns `EventId` and `OccurredOn` at construction.
- [ ] `DefaultOutboxMessageFactory` populates all six metadata fields.
- [ ] `CorrelationId` is consistent across Request → Command → DomainEvent → IntegrationEvent → OutboxMessage.
- [ ] `CausationId` correctly references the triggering command or parent event.
- [ ] `EventId` is unique per event instance.
- [ ] `Producer` identifies the originating module.
- [ ] `Version` is sourced from `OutboxEventAttribute`.
- [ ] `OutboxMessage` exposes all metadata as separate queryable columns.
- [ ] Architecture tests enforce metadata field presence on `OutboxMessage` and `[OutboxEvent]` on all integration events.
- [ ] End-to-end causal chain reconstruction test passes.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── UnitTests/
 │   ├── Hector.BuildingBlocks.Web.UnitTests/
 │   │   └── Correlation/
 │   │       └── CorrelationMetadataPropagationTests.cs              (existing: CorrelationMiddlewareTests.cs — extend)
 │   │
 │   ├── Hector.BuildingBlocks.Application.UnitTests/
 │   │   └── Messaging/
 │   │       └── CorrelationContextAccessorAsyncPropagationTests.cs  (extend existing CorrelationContextAccessorTests.cs)
 │   │
 │   ├── Hector.BuildingBlocks.Domain.UnitTests/
 │   │   └── EventMetadataTests.cs                                   (new — DomainEventBase EventId/OccurredOn)
 │   │
 │   └── Hector.BuildingBlocks.Persistence.UnitTests/
 │       ├── DefaultOutboxMessageFactoryMetadataTests.cs             (new or extend DefaultOutboxMessageFactoryTests.cs)
 │       └── OutboxMessageMetadataFieldsTests.cs                     (new — OutboxMessage field presence)
 │
 ├── IntegrationTests/
 │   ├── Hector.Modules.Projects.IntegrationTests/
 │   │   └── EventCorrelationPropagationTests.cs                     (new — end-to-end chain)
 │   │
 │   └── Hector.BuildingBlocks.Persistence.IntegrationTests/
 │       └── OutboxMessageMetadataPersistenceTests.cs                (new — DB column verification)
 │
 └── ArchitectureTests/
     └── Hector.ArchitectureTests/
         └── EventMetadataArchitectureTests.cs                       (new — NetArchTest guards)
```

## 8. Implementation Notes

- `CorrelationContextAccessor` should use `AsyncLocal<CorrelationContext?>` to guarantee async-flow safety. Tests must verify no cross-request contamination.
- `CorrelationMiddleware` should use the header name defined in `CorrelationHeaderNames` — tests must reference this constant rather than hardcoding strings.
- `DefaultOutboxMessageFactory` should resolve `Version` from `OutboxEventAttribute.Version` via `IOutboxEventTypeResolver` or direct attribute reflection.
- `Producer` should be derived from the event’s containing assembly or module name — tests should verify this is a human-readable module identifier, not a full assembly path.
- `CausationId` propagation requires the application layer handler to explicitly set the parent event’s `EventId` as `CausationId` when mapping domain events to integration events.
- For the end-to-end test, use the shared `TestApplicationFactory` fixture and the `Projects` module’s `CreateProject` command as the representative flow.
- Architecture tests should verify that `OutboxMessage` properties are immutable (init-only setters) to prevent accidental mutation after persistence.

## Summary

This test plan ensures that [ADR-0032](/docs/adr/0032-event-metadata-and-correlation-strategy.md) is enforced through a combination of unit, integration, and architectural tests. It validates that the full metadata set (`EventId`, `CorrelationId`, `CausationId`, `OccurredOn`, `Producer`, `Version`) is consistently populated and propagated across the entire event lifecycle — from HTTP request through commands, domain events, integration events, and outbox persistence. This enables reliable end-to-end tracing, debugging, and observability across the `Hector` modular monolith.
