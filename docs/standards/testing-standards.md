# Testing Standards

## Testing Frameworks

- **Unit Testing:** xUnit
- **Assertion Library:** FluentAssertions
- **Mocking:** NSubstitute
- **Architecture Testing:** NetArchTest.eNet

## Naming Convention

ما از الگوی **`Should_ExpectedBehavior_When_StateUnderTest`** استفاده می‌کنیم.
*مثال: `Should_ReturnTrue_When_ValueObjectsAreEqual`*

## TDD Workflow

1. **Red**: نوشتن تستی که با شکست مواجه می‌شود.
2. **Green**: نوشتن حداقل کد ممکن برای پاس شدن تست.
3. **Refactor**: بهبود ساختار کد بدون تغییر در رفتار.
