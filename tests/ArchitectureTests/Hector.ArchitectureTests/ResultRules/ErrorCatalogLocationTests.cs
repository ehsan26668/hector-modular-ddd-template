using FluentAssertions;
using Hector.BuildingBlocks.Application.Results;
using System.Reflection;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class ErrorCatalogLocationTests
{
    [Fact]
    public void Errors_Should_BeDefined_In_ErrorCatalogClasses()
    {
        // Arrange
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var errorFields = assemblies
            .SelectMany(a => a.GetTypes())
            .SelectMany(t =>
                t.GetFields(
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.FlattenHierarchy)
                .Select(f => new { Field = f, DeclaringType = t }))
            .Where(x => x.Field.FieldType == typeof(Error))
            .ToList();

        var invalidLocations = errorFields
            .Where(x => !IsValidErrorCatalog(x.DeclaringType))
            .Select(x => $"{x.DeclaringType.FullName}.{x.Field.Name}")
            .ToList();

        // Assert
        invalidLocations.Should().BeEmpty(
            "Errors must be declared in static catalog classes located in an '.Errors' namespace and named 'Errors' or '*Errors'. " +
            $"Violations: {string.Join(", ", invalidLocations)}");
    }

    private static bool IsValidErrorCatalog(Type type)
    {
        var isInErrorsNamespace = type.Namespace?.Split('.').Contains("Errors") == true;
        var hasValidName = type.Name == "Errors" || type.Name.EndsWith("Errors", StringComparison.Ordinal);
        var isStaticClass = type.IsAbstract && type.IsSealed;

        return isInErrorsNamespace && hasValidName && isStaticClass;
    }
}
