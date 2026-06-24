using FluentAssertions;
using Hector.BuildingBlocks.Application.Results;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class ErrorCodeNamingConventionTests
{
    private static readonly Regex ErrorCodePattern =
        new(@"^[A-Z]+(_[A-Z]+)+$", RegexOptions.Compiled);

    [Fact]
    public void ErrorCodes_Should_Follow_Naming_Convention()
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

        var invalidCodes = new List<string>();

        // Act
        foreach (var error in errors)
        {
            if (!ErrorCodePattern.IsMatch(error.Code))
            {
                invalidCodes.Add(error.Code);
            }
        }

        // Assert
        invalidCodes.Should().BeEmpty(
            $"Invalid ErrorCode naming detected: {string.Join(", ", invalidCodes)}");
    }
}
