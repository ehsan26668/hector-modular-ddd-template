using FluentAssertions;
using Hector.ArchitectureTests.Common;

namespace Hector.ArchitectureTests.CodingConventions;

public sealed class AssemblyCatalogDiagnosticsTests : ArchitectureTestBase
{
    [Fact]
    public void Should_ContainPersistenceUnitTestsAssembly_When_DiscoveringTestAssemblies()
    {
        // Arrange
        var assemblyNames = TestAssemblies
            .Select(a => a.GetName().Name)
            .OrderBy(x => x)
            .ToList();

        // Act & Assert
        assemblyNames.Should().Contain("Hector.BuildingBlocks.Persistence.UnitTests");
    }
}