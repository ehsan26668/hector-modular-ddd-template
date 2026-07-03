# Test Plan: ADR-0036 Architecture Guard Tests

## Status

Accepted

## Context

This test plan validates the **Architecture Guard Tests** strategy described in [ADR-0036](/docs/adr/0036-architecture-guard-tests.md).

The project follows a modular architecture based on:

- Domain-Driven Design (DDD)
- Clean Architecture
- Modular Monolith principles

The system is organized into feature modules and layered components such as:

- Domain
- Application
- Infrastructure
- Contracts
- Host

As the codebase evolves, architectural drift may occur. Developers may accidentally introduce invalid dependencies or violate layering rules.

Examples include:

- Domain referencing Application or Infrastructure
- Application referencing Infrastructure directly
- Feature modules depending on other feature modules
- StronglyTypedId conventions being bypassed
- Infrastructure concerns leaking into Domain logic

Manual code review alone is insufficient to guarantee architectural integrity at scale.

ADR‑0036 introduces a dedicated architecture test suite that automatically validates architectural constraints during test execution.

These rules are enforced through executable tests using tools such as:

- `NetArchTest.Rules`
- `System.Reflection`

The architecture test suite acts as a safety net that continuously validates architectural boundaries during CI/CD execution.

---

## Test Strategy

Define the layers of testing to be used:

- **Architecture Tests**
  - Validate assembly dependencies, layer boundaries, module isolation, and design conventions.
  - Target Project: `tests/ArchitectureTests/Hector.ArchitectureTests`

- **Unit Tests**
  - Validate helper utilities and reflection-based rule evaluators where applicable.
  - Target Project: `tests/UnitTests`

---

## 1. Scope

### Included

- Domain layer dependency restrictions
- Application layer dependency restrictions
- Infrastructure isolation validation
- Feature module isolation
- StronglyTypedId inheritance enforcement
- Prevention of `Guid.NewGuid()` usage in domain assemblies
- Naming convention enforcement for architecture tests
- Assembly boundary validation

### Excluded

- Runtime behavior validation
- Performance testing
- Infrastructure deployment validation
- External package vulnerability analysis

---

## 2. Test Cases (Architecture / Unit)

### TC-01: Should_NotDependOnApplication_When_InDomainLayer

**Scenario:**  
Domain assemblies must not reference Application assemblies.

**Arrange:**

- Load all domain assemblies using reflection

**Act:**

- Analyze assembly dependencies

**Assert:**

- No domain assembly references any application assembly

---

### TC-02: Should_NotDependOnInfrastructure_When_InDomainLayer

**Scenario:**  
Domain assemblies must not reference Infrastructure assemblies.

**Arrange:**

- Load all domain assemblies

**Act:**

- Inspect referenced assemblies

**Assert:**

- No infrastructure dependency exists

---

### TC-03: Should_NotDependOnInfrastructure_When_InApplicationLayer

**Scenario:**  
Application assemblies must remain infrastructure-agnostic.

**Arrange:**

- Load application assemblies

**Act:**

- Analyze dependency graph

**Assert:**

- No direct infrastructure dependency exists

---

### TC-04: Should_NotDependOnOtherModules_When_InFeatureModule

**Scenario:**  
Feature modules must remain isolated from one another.

**Arrange:**

- Load module assemblies

**Act:**

- Inspect cross-module dependencies

**Assert:**

- Modules do not directly depend on other modules

---

### TC-05: Should_InheritFromStronglyTypedId_When_DeclaringDomainIdentifiers

**Scenario:**  
All domain identifiers must inherit from `StronglyTypedId<>`.

**Arrange:**

- Locate all domain identifier types

**Act:**

- Inspect inheritance hierarchy

**Assert:**

- Every identifier inherits from `StronglyTypedId<>`

---

### TC-06: Should_NotUseGuidNewGuid_When_InDomainLayer

**Scenario:**  
Domain assemblies must not generate identifiers using `Guid.NewGuid()`.

**Arrange:**

- Load domain assemblies

**Act:**

- Inspect IL or reflection metadata

**Assert:**

- No usage of `Guid.NewGuid()` exists

---

### TC-07: Should_FollowArchitectureTestNamingConvention_When_DefiningArchitectureTests

**Scenario:**  
Architecture tests must follow the standard naming convention.

**Arrange:**

- Load architecture test assembly

**Act:**

- Inspect test method names

**Assert:**

- Test names follow:
  `Should_<ExpectedBehavior>_When_<Condition>`

---

### TC-08: Should_KeepInfrastructureConcerns_OutOfDomainLayer

**Scenario:**  
Infrastructure concerns must not leak into domain models.

**Arrange:**

- Load domain assemblies

**Act:**

- Inspect domain types and references

**Assert:**

- No infrastructure-specific abstractions exist in domain layer

---

### TC-09: Should_OnlyReferenceApprovedAssemblies_When_InDomainLayer

**Scenario:**  
Domain assemblies must reference only approved dependencies.

**Arrange:**

- Load domain assemblies

**Act:**

- Analyze referenced assemblies

**Assert:**

- Only approved framework and domain dependencies exist

---

### TC-10: Should_FailArchitectureTests_When_RulesAreViolated

**Scenario:**  
Architectural violations must fail the test suite.

**Arrange:**

- Introduce intentional invalid dependency

**Act:**

- Execute architecture tests

**Assert:**

- Test suite fails
- Violation details are reported clearly

---

## 3. Non-Functional Validation Points

### 3.1 Maintainability

- Verify architecture rules remain readable and maintainable.
- Ensure new modules can integrate into architecture tests easily.

### 3.2 Reliability

- Verify architecture tests consistently detect violations.
- Verify reflection-based rules remain deterministic.

### 3.3 CI/CD Integration

- Verify architecture tests execute through:
  `dotnet test`
- Verify failures block invalid architectural changes.

---

## 4. Test Data

### Inputs

- Domain assemblies
- Application assemblies
- Infrastructure assemblies
- Feature module assemblies
- Architecture test assemblies

### Expected Outputs

- Invalid dependencies detected
- Architectural violations fail tests
- Layering rules enforced automatically
- Naming conventions validated

---

## 5. TDD Execution Plan

### 1. RED

- Write failing architecture tests for dependency violations.
- Introduce intentional invalid references to validate detection.

### 2. GREEN

- Implement architecture rules using:
  - `NetArchTest.Rules`
  - reflection-based inspection

### 3. REFACTOR

- Extract reusable architecture rule helpers.
- Centralize assembly loading logic.
- Improve readability of rule assertions.

---

## 6. Exit Criteria

- [x] All Architecture Tests pass.
- [x] All architectural boundaries verified.
- [x] CI/CD integration validated.
- [x] Documentation updated.

---

## 7. Proposed Test File Layout

```text
tests/ArchitectureTests/Hector.ArchitectureTests/
 ├── Common/                            # تعاریف پایه‌ای، ثابت‌ها و متدهای کمکی برای بارگذاری اسمبلی‌ها
 │   ├── AssemblyReference.cs           # نگهدارنده اسمبلی‌های پروژه جهت دسترسی ساده در تست‌ها
 │   └── BaseArchitectureTests.cs       # (اختیاری) کلاس پایه برای تعاریف مشترک
 │
 ├── LayerBoundaries/                   # تست‌های مربوط به مرزها و وابستگی‌های بین لایه‌ای (Clean Architecture)
 │   ├── LayerDependencyTests.cs        # تست‌های عدم وابستگی Domain به Application/Infrastructure و ...
 │   └── ModuleIsolationTests.cs        # تست‌های مربوط به ایزولاسیون کامل ماژول‌ها از همدیگر
 │
 ├── DomainRules/                       # قوانین و استانداردهای طراحی درون لایه Domain
 │   ├── DomainIdentityTests.cs         # تست عدم استفاده از Guid.NewGuid و استانداردهای شناسه
 │   └── StronglyTypedIdTests.cs        # تست ارث‌بری تمام شناسه‌ها از StronglyTypedId<>
 │
 ├── ApplicationRules/                  # قوانین مربوط به CQRS، هندلرها و الگوهای لایه Application
 │   ├── CommandHandlerTests.cs         # الزامات طراحی کامندها (بازگشت Result، ساید افکت‌ها و...)
 │   ├── QueryHandlerTests.cs           # الزامات طراحی کوئری‌ها (عدم تغییر وضعیت، عدم استفاده از مخازن نامعتبر)
 │   └── QueryResponseTypeTests.cs      # بررسی نوع خروجی کوئری‌ها بر اساس Result pattern
 │
 └── CodingConventions/                 # تست‌های مربوط به سبک کدنویسی و قراردادهای ساختاری خود تست‌ها
     └── ArchitectureTestNamingTests.cs # تست نام‌گذاری متدهای تست معماری بر اساس الگو
```

## Summary

This test plan ensures that [ADR‑0036](/docs/adr/0036-architecture-guard-tests.md) continuously protects the architectural integrity of the system.

By enforcing architectural constraints through executable tests, the project prevents architectural drift, preserves modular boundaries, and increases confidence during long‑term evolution and refactoring of the codebase.
