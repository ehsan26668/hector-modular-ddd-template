using FluentAssertions;

namespace Hector.ArchitectureTests.Framework.Unit.RuleTests;

[Trait("Category", "ArchitectureTests")]
public sealed class ArchitectureRuleTests
{
    [Fact(DisplayName = "ADR-0056 | TC-01 | Should create architecture rule with metadata")]
    public void Should_CreateArchitectureRuleWithMetadata_When_Constructed()
    {
        // Arrange
        const string ruleId = "ADR-0056-001";
        const string ruleName = "Application layer must remain independent";
        const string because = "application layer must remain transport-agnostic";

        // Act
        var rule = new ArchitectureRule(
            ruleId,
            ruleName,
            because,
            static () => { });

        // Assert
        rule.Id.Should().Be(ruleId);
        rule.Name.Should().Be(ruleName);
        rule.Reason.Should().Be(because);
    }

    [Fact(DisplayName = "ADR-0056 | TC-01 | Should execute underlying assertion")]
    public void Should_ExecuteUnderlyingAssertion_When_Evaluated()
    {
        // Arrange
        var wasExecuted = false;

        var rule = new ArchitectureRule(
            "ADR-0056-002",
            "Dummy rule",
            "rules must execute underlying assertion",
            () => wasExecuted = true);

        // Act
        rule.Evaluate();

        // Assert
        wasExecuted.Should().BeTrue();
    }
}
