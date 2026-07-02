using FluentAssertions;
using Hector.BuildingBlocks.Application.Results;
using System.Reflection;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class ErrorCodeUniquenessTests
{
    [Fact]
    public void Should_BeUnique_When_DefiningErrorCodes()
    {
        // Arrange
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var errorFields = assemblies
            .SelectMany(a => a.GetTypes())
            .SelectMany(t =>
                t.GetFields(BindingFlags.Public |
                            BindingFlags.Static |
                            BindingFlags.FlattenHierarchy))
            .Where(f => f.FieldType == typeof(Error))
            .ToList();

        var errors = errorFields
            .Select(f => (Error)f.GetValue(null)!)
            .ToList();

        // Act
        var duplicates = errors
            .GroupBy(e => e.Code)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        // Assert
        duplicates.Should().BeEmpty(
            $"Duplicate Error Codes detected: {string.Join(", ", duplicates)}");
    }
}
