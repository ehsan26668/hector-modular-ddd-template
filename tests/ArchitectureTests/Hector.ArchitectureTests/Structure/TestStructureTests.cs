using FluentAssertions;

namespace Hector.ArchitectureTests.Structure;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0002")]
[Trait("Validation", "TestStructure")]
public sealed class TestStructureTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string TestsDirectory = Path.Combine(RepositoryRoot, "tests");

    [Fact(DisplayName = "ADR-0002 | TC-09 | Test area must be structured into standardized categories")]
    public void Should_ContainStandardTestCategories_When_TestStructureIsValidated()
    {
        // Arrange
        var expectedTestFolders = new[]
        {
            "ArchitectureTests",
            "UnitTests",
            "IntegrationTests",
            "TemplateTests",
            "Shared"
        };
        var violations = new List<string>();

        // Act
        if (!Directory.Exists(TestsDirectory))
        {
            violations.Add($"Tests root directory was not found at: {TestsDirectory}");
        }
        else
        {
            foreach (var folder in expectedTestFolders)
            {
                var targetPath = Path.Combine(TestsDirectory, folder);
                if (!Directory.Exists(targetPath))
                {
                    violations.Add($"Missing test category directory: 'tests/{folder}'");
                }
            }
        }

        // Assert
        violations.Should().BeEmpty(
            "tests must be organized cleanly into ArchitectureTests, UnitTests, IntegrationTests, TemplateTests, and Shared testing infrastructure.");
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var solutionFile = Path.Combine(directory.FullName, "Hector.slnx");
            if (File.Exists(solutionFile))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }
        throw new InvalidOperationException("Could not locate repository root containing Hector.slnx.");
    }
}
