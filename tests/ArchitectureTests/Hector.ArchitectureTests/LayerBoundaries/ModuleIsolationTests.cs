using FluentAssertions;
using Hector.ArchitectureTests.Common;
using Hector.ArchitectureTests.Framework;

namespace Hector.ArchitectureTests.LayerBoundaries;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0002")]
[Trait("Validation", "ModuleIsolation")]
public sealed class ModuleIsolationTests : ArchitectureTestBase
{
    [Fact(DisplayName = "ADR-0002 | TC-04 | Feature modules must be isolated and communicate only through Contracts")]
    public void Should_OnlyAllowCrossModuleCommunicationThroughContracts_When_ComparingFeatureModules()
    {
        // Arrange
        var rule = ArchitectureRule
            .Modules()
            .From(ModuleAssemblies)
            .AllowCrossModuleDependenciesOnlyThroughContracts()
            .Build("ADR-0002-TC-04", "Feature module isolation")
            .Because("feature modules must not reference each other directly; all cross-module communication must happen through public Contracts.");

        // Act
        var result = rule.EvaluateWithResult();

        // Assert
        result.HasViolations.Should().BeFalse(
            $"Cross-module dependencies must go through Contracts only. Violations:{Environment.NewLine}{string.Join(Environment.NewLine, result.Diagnostics)}");
    }
}
