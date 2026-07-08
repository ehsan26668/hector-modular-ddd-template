using FluentAssertions;

namespace Hector.ArchitectureTests.Structure;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0002")]
[Trait("Validation", "ProjectStructure")]
public sealed class ModuleStructureTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string ModulesDirectory = Path.Combine(RepositoryRoot, "src", "Modules");

    [Fact(DisplayName = "ADR-0002 | TC-06 | Each module must contain the standard four-layer structure")]
    public void Should_ContainStandardLayers_When_ModuleExists()
    {
        // Arrange
        if (!Directory.Exists(ModulesDirectory))
        {
            throw new InvalidOperationException($"Modules directory was not found: {ModulesDirectory}");
        }

        var expectedLayers = new[]
        {
            "Domain",
            "Application",
            "Infrastructure",
            "Contracts"
        };

        var moduleDirectories = Directory.GetDirectories(ModulesDirectory);
        var violations = new List<string>();

        // Act
        foreach (var moduleDirectory in moduleDirectories)
        {
            var moduleName = Path.GetFileName(moduleDirectory);

            foreach (var layer in expectedLayers)
            {
                var layerDirectory = Path.Combine(moduleDirectory, layer);

                if (!Directory.Exists(layerDirectory))
                {
                    violations.Add($"{moduleName}: Missing layer directory 'src/Modules/{moduleName}/{layer}'");
                    continue;
                }

                var expectedProjectFile = Path.Combine(layerDirectory, $"Hector.Modules.{moduleName}.{layer}.csproj");

                if (!File.Exists(expectedProjectFile))
                {
                    violations.Add($"{moduleName}: Missing project file 'src/Modules/{moduleName}/{layer}/Hector.Modules.{moduleName}.{layer}.csproj'");
                }
            }
        }

        // Assert
        violations.Should().BeEmpty(
            "each feature module under src/Modules/ must follow the standard 4-layer structure: Domain, Application, Infrastructure, and Contracts with corresponding project files.");
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
