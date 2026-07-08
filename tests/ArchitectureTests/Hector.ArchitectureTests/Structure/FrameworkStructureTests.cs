using FluentAssertions;

namespace Hector.ArchitectureTests.Structure;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0002")]
[Trait("Validation", "FrameworkStructure")]
public sealed class FrameworkStructureTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string FrameworkDirectory = Path.Combine(RepositoryRoot, "src", "Framework");

    [Fact(DisplayName = "ADR-0002 | TC-08 | Framework area must contain all expected building block and testing projects")]
    public void Should_ContainExpectedFrameworkProjects_When_SolutionStructureIsValidated()
    {
        // Arrange
        var expectedProjects = new[]
        {
            "Hector.BuildingBlocks.Domain",
            "Hector.BuildingBlocks.Application",
            "Hector.BuildingBlocks.Persistence",
            "Hector.BuildingBlocks.Web",
            "Hector.ArchitectureTests.Framework"
        };
        var violations = new List<string>();

        // Act
        if (!Directory.Exists(FrameworkDirectory))
        {
            violations.Add($"Framework root directory was not found at: {FrameworkDirectory}");
        }
        else
        {
            foreach (var projectName in expectedProjects)
            {
                var projectFolder = Path.Combine(FrameworkDirectory, projectName);
                var projectFile = Path.Combine(projectFolder, $"{projectName}.csproj");

                if (!Directory.Exists(projectFolder))
                {
                    violations.Add($"Missing directory: 'src/Framework/{projectName}'");
                    continue;
                }

                if (!File.Exists(projectFile))
                {
                    violations.Add($"Missing project file: 'src/Framework/{projectName}/{projectName}.csproj'");
                }
            }
        }

        // Assert
        violations.Should().BeEmpty(
            "the Framework layer must provide the essential domain, application, persistence, web building blocks and the architecture testing framework.");
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
