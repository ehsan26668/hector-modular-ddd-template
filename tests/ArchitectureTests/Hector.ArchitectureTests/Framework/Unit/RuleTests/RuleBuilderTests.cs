using System.Reflection;
using FluentAssertions;

namespace Hector.ArchitectureTests.Framework.Unit.RuleTests;

[Trait("Category", "ArchitectureTests")]
public sealed class RuleBuilderTests
{
    [Fact(DisplayName = "ADR-0056 | TC-02 | Should detect dependency violations using DSL")]
    public void Should_ReportViolations_When_DependencyBoundaryIsBroken()
    {
        // Arrange
        // Create a rule: Domain should not depend on Infrastructure
        var targetAssembly = Assembly.GetExecutingAssembly();

        var rule = ArchitectureRule
            .Types()
            .That(targetAssembly)
                .ResideInNamespace("Hector.ArchitectureTests.Framework.Unit")
            .Should()
                .NotDependOn("FluentAssertions")
            .Build("ADR-0056-TC02", "Tests must not depend directly on FluentAssertions")
            .Because("We want to test violation capturing");

        // Act
        var result = rule.EvaluateWithResult();

        // Assert
        result.HasViolations.Should().BeTrue("because test classes explicitly reference FluentAssertions");
        result.Diagnostics.Should().NotBeEmpty();
        result.Diagnostics.Any(d => d.Contains("FluentSyntaxTests")).Should().BeTrue();
    }
}