using FluentAssertions;

namespace Hector.ArchitectureTests.Structure;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0002")]
[Trait("Validation", "NamingConventions")]
public sealed class ProjectNamingConventionTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact(DisplayName = "ADR-0002 | TC-11 | Projects must follow naming and placement conventions")]
    public void Should_FollowProjectNamingAndPlacementConventions_When_SolutionStructureIsValidated()
    {
        // Arrange
        var srcDirectory = Path.Combine(RepositoryRoot, "src");
        var testsDirectory = Path.Combine(RepositoryRoot, "tests");
        var violations = new List<string>();

        var validTestSuffixes = new[] { ".Tests", ".UnitTests", ".IntegrationTests", ".TemplateTests" };
        var allowedTestHelperProjects = new[] { "Hector.ArchitectureTests", "Hector.Testing", "Hector.Persistence.Testing" };

        // Act
        var allProjectFiles = Directory.GetFiles(RepositoryRoot, "*.csproj", SearchOption.AllDirectories);

        foreach (var projectFile in allProjectFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(projectFile);
            var relativePath = Path.GetRelativePath(RepositoryRoot, projectFile);

            // 1. Host Project Rules
            if (relativePath.StartsWith("src" + Path.DirectorySeparatorChar + "Host"))
            {
                if (fileName != "Hector.Host")
                    violations.Add($"Host project at '{relativePath}' must be named 'Hector.Host'.");
            }
            // 2. Framework Project Rules
            else if (relativePath.StartsWith("src" + Path.DirectorySeparatorChar + "Framework"))
            {
                if (!fileName.StartsWith("Hector.BuildingBlocks.") && !fileName.Equals("Hector.ArchitectureTests.Framework"))
                    violations.Add($"Framework project at '{relativePath}' follows invalid naming: '{fileName}'.");
            }
            // 3. Module Project Rules
            else if (relativePath.StartsWith("src" + Path.DirectorySeparatorChar + "Modules"))
            {
                if (!fileName.StartsWith("Hector.Modules."))
                    violations.Add($"Module project at '{relativePath}' must start with 'Hector.Modules.'. Found: '{fileName}'.");

                // Validate 4-layer suffix
                var parts = fileName.Split('.');
                var layer = parts.Last();
                var validLayers = new[] { "Domain", "Application", "Infrastructure", "Contracts" };
                if (!validLayers.Contains(layer))
                    violations.Add($"Module project '{fileName}' has invalid layer suffix. Expected one of: {string.Join(", ", validLayers)}.");
            }
            // 4. Test Project Rules
            else if (relativePath.StartsWith("tests"))
            {
                if (allowedTestHelperProjects.Contains(fileName))
                    continue;

                var hasValidSuffix = validTestSuffixes.Any(suffix => fileName.EndsWith(suffix));
                if (!fileName.StartsWith("Hector.") || !hasValidSuffix)
                {
                    violations.Add($"Test project at '{relativePath}' must follow naming pattern (e.g. 'Hector.*.UnitTests' or 'Hector.*.IntegrationTests'). Found: '{fileName}'.");
                }
            }
        }

        // Assert
        violations.Should().BeEmpty("all projects must adhere to the standardized naming and directory structure defined in the Modular DDD architecture.");
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
