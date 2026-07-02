using Hector.ArchitectureTests.Common;
using Hector.Modules.Projects.Domain;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.LayerBoundaries;

[Trait("Category", "ArchitectureTests")]
public sealed class ModuleIsolationTests : ArchitectureTestBase
{
    [Fact(DisplayName = "ADR-0036 | TC-04 | Should not depend on other modules")]
    public void Should_NotDependOnOtherModules_When_InFeatureModule()
    {
        // Arrange
        var projectsDomainAssembly =
            GetModuleDomainAssembly("Projects", typeof(ProjectsDomainAssemblyMarker));

        string[] otherModuleNamespaces = ["Hector.Modules.Billing", "Hector.Modules.Customers"];

        // Act
        var result = Types.InAssembly(projectsDomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherModuleNamespaces)
            .GetResult();

        // Assert
        result.AssertSuccessful("Feature modules must remain isolated from each other.");
    }
}
