using FluentAssertions;
using Hector.BuildingBlocks.Application.Results;
using System.Reflection;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class CommandErrorCategoryPolicyTests
{
    private static readonly ErrorCategory[] AllowedCategories =
    [
        ErrorCategory.Validation,
        ErrorCategory.Conflict,
        ErrorCategory.Infrastructure
    ];

    [Fact]
    public void Should_UseAllowedCategories_When_DefiningCommandErrors()
    {
        // Arrange
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // فیلتر کردن فقط کلاس‌های خطای مربوط به دستورات (Commands)
        var errors = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.Namespace != null && t.Namespace.Contains("Errors.Commands"))
            .SelectMany(t => t.GetFields(
                BindingFlags.Public |
                BindingFlags.Static |
                BindingFlags.FlattenHierarchy))
            .Where(f => f.FieldType == typeof(Error))
            .Select(f => (Error)f.GetValue(null)!)
            .ToList();

        // Act
        var invalidErrors = errors
            .Where(e => !AllowedCategories.Contains(e.Category))
            .Select(e => $"{e.Code} ({e.Category})")
            .ToList();

        // Assert
        invalidErrors.Should().BeEmpty(
            $"Command Errors must use only allowed categories. Violations: {string.Join(", ", invalidErrors)}");
    }

}
