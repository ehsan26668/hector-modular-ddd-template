using FluentAssertions;

namespace Hector.ArchitectureTests.Structure;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0002")]
[Trait("Validation", "HostStructure")]
public sealed class HostStructureTests
{
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Fact(DisplayName = "ADR-0002 | TC-7 | Unified host project must exist at the designated path")]
    public void Should_ContainUnifiedHostProject_When_SolutionStructureIsValidated()
    {
        // Arrange
        var expectedHostProjectDir = Path.Combine(RepositoryRoot, "src", "Host", "Hector.Host");
        var expectedHostProjectFile = Path.Combine(expectedHostProjectDir, "Hector.Host.csproj");

        // Act
        var directoryExists = Directory.Exists(expectedHostProjectDir);
        var fileExists = File.Exists(expectedHostProjectFile);

        // Assert
        directoryExists.Should().BeTrue("the Host directory 'src/Host/Hector.Host' must exist as defined in ADR-0002.");
        fileExists.Should().BeTrue("the Host composition root project file 'Hector.Host.csproj' must exist inside the Host directory.");
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