# Test Plan: ADR-0046 Framework and Product Co‑Evolution Strategy

## Status

Accepted

## Context

This test plan validates the **Framework/Product Co‑Evolution strategy** defined in [ADR-0046](/docs/adr/0046-framework-product-coevolution-strategy.md).

Hector is intended to evolve into a production-ready architectural framework for modular monolith systems based on DDD, CQRS, Outbox, and Inbox patterns.

To ensure that the framework evolves based on real-world usage, a real product called **Accounting** will be developed using Hector.

Two independent repositories will exist:

Framework repository:

Hector

Product repository:

Accounting

The co-evolution model ensures that the framework evolves based on practical needs while remaining domain-agnostic.

Accounting will always be generated from the Hector template. When missing capabilities are discovered during product development, improvements must first be implemented in Hector before being consumed by Accounting.

This test plan ensures that:

- Hector remains domain-agnostic
- Accounting does not introduce framework logic
- Accounting is generated from the Hector template
- improvements flow from Hector to Accounting
- repository boundaries are respected

Maintaining these constraints is critical to ensuring that Hector evolves as a reusable architectural framework rather than becoming tightly coupled to the Accounting product.

## Test Strategy

Define the layers of testing to be used:

- **Architecture Tests**
  - Ensure Hector remains domain-agnostic.
  - Prevent domain-specific modules from appearing in the framework repository.
  - Target Project: `tests/ArchitectureTests`

- **Template Validation Tests**
  - Ensure Accounting solutions are generated from the Hector template.
  - Target Project: `tests/IntegrationTests/Hector.Template.IntegrationTests`

- **Process Validation Tests**
  - Validate that framework capabilities originate in Hector before being used in Accounting.
  - Target Project: `tests/ArchitectureTests`

---

## 1. Scope

- **Included:**
  - Framework and product repository separation
  - Domain isolation validation
  - Template-based product initialization
  - Architectural boundary enforcement
  - Framework evolution workflow validation

- **Excluded:**
  - Accounting business logic tests
  - Accounting domain validation
  - CI/CD repository pipelines
  - Organizational workflow management

---

## 2. Test Cases (Unit / Integration / Architecture)

### TC-01: Should_EnsureFrameworkRepositoryIsDomainAgnostic

**Scenario:**  
The Hector framework must not contain any product-specific domain logic.

**Arrange:**

Scan the Hector solution for domain-specific keywords or modules such as:

- Accounting
- Ledger
- Invoice
- Payment
- Tax

**Act:**

Run architecture validation tests across the framework projects.

**Assert:**

- Verify no domain-specific modules exist inside Hector.
- Verify framework modules remain generic and reusable.
- Verify architectural boundaries are preserved.

---

### TC-02: Should_PreventProductDomainLogicInsideFramework

**Scenario:**  
Product-specific business logic must never appear in the Hector repository.

**Arrange:**

Analyze framework assemblies for business-domain entities or aggregates.

**Act:**

Run architecture tests inspecting namespace boundaries.

**Assert:**

- Verify no Accounting domain entities exist in Hector.
- Verify no product-specific aggregates appear in framework assemblies.

---

### TC-03: Should_GenerateAccountingSolutionFromHectorTemplate

**Scenario:**  
The Accounting product must always be initialized using the Hector template.

**Arrange:**

Install the Hector template.

**Act:**

Execute:

dotnet new hectorddd -n Accounting

**Assert:**

- Verify Accounting solution structure is generated correctly.
- Verify the generated project follows the Hector modular architecture.
- Verify namespace replacement is correct.

---

### TC-04: Should_EnsureFrameworkCapabilitiesExistBeforeProductUsage

**Scenario:**  
New infrastructure capabilities must originate in Hector before being used in Accounting.

**Arrange:**

Identify a capability introduced in Accounting.

**Act:**

Trace the capability origin to the Hector framework.

**Assert:**

- Verify the capability exists in Hector first.
- Verify Accounting only consumes the capability.
- Verify framework-first evolution rule is respected.

---

### TC-05: Should_EnsureAccountingDoesNotModifyFrameworkCode

**Scenario:**  
The Accounting repository must not modify or fork framework code.

**Arrange:**

Analyze Accounting repository dependencies.

**Act:**

Inspect framework usage.

**Assert:**

- Verify Accounting references Hector as a framework dependency.
- Verify no Hector source code is embedded in Accounting.
- Verify Accounting does not override framework components.

---

### TC-06: Should_EnsureFrameworkUpdatesCanBeIntegratedIntoAccounting

**Scenario:**  
Accounting must be able to integrate improvements introduced in Hector.

**Arrange:**

Create a new Hector template version.

**Act:**

Upgrade Accounting to the newer template version.

**Assert:**

- Verify the upgrade succeeds without structural conflicts.
- Verify new framework capabilities become available.
- Verify product modules remain unaffected.

---

### TC-07: Should_EnsureTemplateVersionIncrementWhenFrameworkChanges

**Scenario:**  
Framework changes must trigger template version increments.

**Arrange:**

Modify framework template structure.

**Act:**

Create a new template release.

**Assert:**

- Verify template version is incremented.
- Verify new template version can be installed.
- Verify project generation reflects the update.

---

## 3. Non-Functional Validation Points

### 3.1 Architectural Integrity

- Verify framework remains domain-agnostic.
- Verify separation between framework and product repositories.

### 3.2 Maintainability

- Verify framework improvements are reusable across products.
- Verify template upgrades remain manageable.

### 3.3 Evolvability

- Verify the framework can evolve independently.
- Verify product development does not break architectural boundaries.

---

## 4. Test Data

- **Inputs:**
  - Hector template package
  - Accounting solution generated from template
  - Template version updates

- **Expected Outputs:**
  - Domain-agnostic framework
  - Product solution generated from template
  - Successful framework upgrade path
  - No framework-product coupling

---

## 5. TDD Execution Plan

Outline the steps for Red-Green-Refactor:

1. **RED**

Write failing architecture tests that detect:

- domain logic inside Hector
- missing template generation
- framework capability used before framework implementation

1. **GREEN**

Implement:

- architecture rules
- template validation
- repository boundary checks

1. **REFACTOR**

Simplify architecture tests and enforce consistent framework boundaries.

---

## 6. Exit Criteria

- [ ] Architecture tests pass.
- [ ] Template generation validated.
- [ ] Framework remains domain-agnostic.
- [ ] Product uses framework without modifying it.
- [ ] Framework upgrades validated.

---

## 7. Proposed Test File Layout

```text
tests/
 ├── ArchitectureTests/
 │   ├── FrameworkDomainIsolationTests.cs
 │   ├── FrameworkBoundaryTests.cs
 │   └── ProductFrameworkUsageTests.cs
 │
 └── IntegrationTests/
     └── Hector.Template.IntegrationTests/
         └── AccountingTemplateGenerationTests.cs
```

## Summary

This test plan validates the Framework/Product Co‑Evolution strategy defined in ADR-0046.

It ensures that:

- Hector remains a reusable architectural framework
- Accounting evolves as a real product built on top of the framework
- framework and product repositories remain properly separated
- improvements flow from framework to product

By enforcing these rules, Hector evolves through real-world usage while maintaining architectural integrity and reusability.
