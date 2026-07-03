using System.Reflection;
using FluentAssertions;

namespace Hector.ArchitectureTests.Framework.Unit;

[Trait("Category", "ArchitectureTests")]
public sealed class ConventionPackTests
{
    [Fact(DisplayName = "ADR-0056 | TC-03 | Should execute convention pack when using predefined policies")]
    public void Should_ExecuteConventionPack_When_UsingPredefinedPolicies()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();
        var conventionPack = Conventions.LayerIsolation(assembly);

        // Act
        var report = conventionPack.Evaluate();

        // Assert
        report.Should().NotBeNull();
        report.Violations.Should().NotBeNull();
    }

    [Fact(DisplayName = "ADR-0056 | TC-03 | Should aggregate diagnostics when multiple rules fail")]
    public void Should_AggregateDiagnostics_When_MultipleRulesFail()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet()
            .Add(new ArchitectureRule(
                "ADR-0056-AGG-002",
                "Second failing rule",
                "Validation of aggregation behavior",
                () => EvaluationResult.Failure(["Second diagnostic error."])))
            .Add(new ArchitectureRule(
                "ADR-0056-AGG-001",
                "First failing rule",
                "Validation of aggregation behavior",
                () => EvaluationResult.Failure(["First diagnostic error."])));

        // Act
        var report = ruleSet.Evaluate();

        // Assert
        report.HasViolations.Should().BeTrue();
        report.Violations.Should().HaveCount(2);
        report.Violations.Select(v => v.RuleId).Should().Equal(
            "ADR-0056-AGG-001",
            "ADR-0056-AGG-002");
    }

    [Fact(DisplayName = "ADR-0056 | TC-03 | Should execute rules in deterministic order when convention pack runs")]
    public void Should_ExecuteRulesInDeterministicOrder_When_ConventionPackRuns()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet()
            .Add(new ArchitectureRule(
                "ADR-0056-ORDER-003",
                "Third rule",
                "Validation of execution ordering",
                () => EvaluationResult.Failure(["Error C"])))
            .Add(new ArchitectureRule(
                "ADR-0056-ORDER-001",
                "First rule",
                "Validation of execution ordering",
                () => EvaluationResult.Failure(["Error A"])))
            .Add(new ArchitectureRule(
                "ADR-0056-ORDER-002",
                "Second rule",
                "Validation of execution ordering",
                () => EvaluationResult.Failure(["Error B"])));

        // Act
        var firstRunReport = ruleSet.Evaluate();
        var secondRunReport = ruleSet.Evaluate();

        // Assert
        firstRunReport.Violations.Select(v => v.RuleId).Should().Equal(
            "ADR-0056-ORDER-001",
            "ADR-0056-ORDER-002",
            "ADR-0056-ORDER-003");

        secondRunReport.Violations.Select(v => v.RuleId)
            .Should().Equal(firstRunReport.Violations.Select(v => v.RuleId));
    }
}