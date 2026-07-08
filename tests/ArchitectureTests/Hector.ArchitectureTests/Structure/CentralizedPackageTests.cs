using System.Xml.Linq;
using FluentAssertions;

namespace Hector.ArchitectureTests.Structure;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0002")]
[Trait("Validation", "CentralPackageManagement")]
public sealed class CentralizedPackageTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact(DisplayName = "ADR-0002 | TC-05 | Projects must use Central Package Management through Directory.Packages.props")]
    public void Should_UseCentralPackageManagement_When_DefiningPackageReferences()
    {
        // Arrange
        var directoryPackagesPropsPath = Path.Combine(RepositoryRoot, "Directory.Packages.props");
        var projectFiles = Directory
            .EnumerateFiles(RepositoryRoot, "*.csproj", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) &&
                           !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var violations = new List<string>();

        // Act
        File.Exists(directoryPackagesPropsPath).Should().BeTrue(
            "ADR-0002 requires centralized package management through Directory.Packages.props at the repository root.");

        foreach (var projectFile in projectFiles)
        {
            var document = XDocument.Load(projectFile);
            var packageReferences = document.Descendants("PackageReference");

            foreach (var reference in packageReferences)
            {
                var packageName = reference.Attribute("Include")?.Value ?? "UnknownPackage";
                var versionAttribute = reference.Attribute("Version");
                var versionElement = reference.Element("Version");
                var versionOverrideAttribute = reference.Attribute("VersionOverride");

                if (versionAttribute is not null)
                {
                    violations.Add($"{Path.GetRelativePath(RepositoryRoot, projectFile)}: Package '{packageName}' defines inline Version attribute.");
                }

                if (versionElement is not null)
                {
                    violations.Add($"{Path.GetRelativePath(RepositoryRoot, projectFile)}: Package '{packageName}' defines inline Version element.");
                }

                if (versionOverrideAttribute is not null)
                {
                    violations.Add($"{Path.GetRelativePath(RepositoryRoot, projectFile)}: Package '{packageName}' uses forbidden VersionOverride.");
                }
            }
        }

        // Assert
        violations.Should().BeEmpty(
            "ADR-0002 enforces Central Package Management. Package versions must be declared only in Directory.Packages.props.");
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
