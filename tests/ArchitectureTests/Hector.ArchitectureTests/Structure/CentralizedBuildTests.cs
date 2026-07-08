using FluentAssertions;

namespace Hector.ArchitectureTests.Structure;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0002")]
[Trait("Validation", "CentralizedBuild")]
public sealed class CentralizedBuildTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact(DisplayName = "ADR-0002 | TC-10 | Solution must contain centralized build and package configuration files")]
    public void Should_ContainCentralizedBuildFiles_When_SolutionStructureIsValidated()
    {
        // Arrange
        var requiredFiles = new[]
        {
            "Directory.Build.props",
            "Directory.Packages.props"
        };
        var violations = new List<string>();

        // Act
        foreach (var fileName in requiredFiles)
        {
            var filePath = Path.Combine(RepositoryRoot, fileName);
            if (!File.Exists(filePath))
            {
                violations.Add($"Missing centralized configuration file: '{fileName}' at repository root.");
            }
        }

        // Assert
        violations.Should().BeEmpty(
            "centralized build logic and dependency management (CPM) are mandatory for maintaining consistency across all projects.");
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
