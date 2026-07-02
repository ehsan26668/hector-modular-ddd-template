using Hector.ArchitectureTests.Common;

namespace Hector.ArchitectureTests.LayerBoundaries;

[Trait("Category", "ArchitectureTests")]
public sealed class LayerDependencyTests : ArchitectureTestBase
{
    [Fact(DisplayName = "ADR-0036 | TC-01 | Should not depend on application")]
    public void Should_NotDependOnApplication_When_InDomainLayer()
    {
        // Arrange
        var forbidden = ApplicationAssemblies.Select(a => a.GetName().Name!).ToArray();

        // Act & Assert
        ArchitectureAssertions.ShouldNotDependOnAny(
            DomainAssemblies,
            forbidden,
            because: "Domain assemblies must not reference Application assemblies.");
    }

    [Fact(DisplayName = "ADR-0036 | TC-02 | Should not depend on infrastructure")]
    public void Should_NotDependOnInfrastructure_When_InDomainLayer()
    {
        // Arrange
        var forbidden = InfrastructureAssemblies.Select(a => a.GetName().Name!).ToArray();

        // Act & Assert
        ArchitectureAssertions.ShouldNotDependOnAny(
            DomainAssemblies,
            forbidden,
            because: "Domain assemblies must not reference Infrastructure assemblies.");
    }

    [Fact(DisplayName = "ADR-0036 | TC-03 | Should not depend on infrastructure")]
    public void Should_NotDependOnInfrastructure_When_InApplicationLayer()
    {
        // Arrange
        var forbidden = InfrastructureAssemblies.Select(a => a.GetName().Name!).ToArray();

        // Act & Assert
        ArchitectureAssertions.ShouldNotDependOnAny(
            ApplicationAssemblies,
            forbidden,
            because: "Application assemblies must not reference Infrastructure assemblies.");
    }
}
