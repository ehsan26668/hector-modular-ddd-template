using FluentAssertions;
using Hector.ArchitectureTests.Common;
using Hector.ArchitectureTests.Framework;

namespace Hector.ArchitectureTests.LayerBoundaries;

[Trait("Category", "ArchitectureTests")]
[Trait("ADR", "ADR-0057")]
[Trait("Validation", "Framework")]
public sealed class MultiAssemblyRuleEvaluationTests
{
    [Fact(DisplayName = "ADR-0057 | TC-03 | Should report violations across all assemblies when rule fails on multiple assemblies")]
    public void Should_ReportViolationsAcrossAllAssemblies_When_RuleFailsOnMultipleAssemblies()
    {
        // Arrange
        var domainAssembly = AssemblyCatalog.Domain.First();
        var applicationAssembly = AssemblyCatalog.Application.First();
        var targetAssemblies = new[] { domainAssembly, applicationAssembly };

        // We intentionally set up a rule that is guaranteed to fail in both assemblies 
        // by asserting they must not depend on "System" (which is commonly used in both domain & application code)
        var forbiddenDependency = "System";

        var rule = ArchitectureRule.Types()
            .That(targetAssemblies)
            .Should()
            .NotDependOn(forbiddenDependency)
            .Build(
                "TEST-MULTI-ASM-01",
                "Violation test for multi-assembly rule dependency enforcement"
            )
            .Because("we want to ensure violations from all target assemblies are aggregated and reported in the failure");

        // Act
        var act = () => rule.Evaluate();

        // Assert
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage($"*{domainAssembly.GetName().Name}*", "because violation diagnostic reports must list types from the failing Domain assembly").And.Message
           .Should()
           .Contain(applicationAssembly.GetName().Name, "because violation diagnostic reports must also list types from the failing Application assembly");
    }
}