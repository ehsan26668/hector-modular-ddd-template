using FluentAssertions;

namespace Hector.ArchitectureTests.Framework.Unit;

[Trait("Category", "ArchitectureTests")]
public sealed class ArchitectureDiagnosticsTests
{
    [Fact(DisplayName = "ADR-0056 | NFR-01 | Should include rule metadata in diagnostics for traceability")]
    public void Should_IncludeRuleMetadata_When_ViolationIsReported()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet()
            .Add(new ArchitectureRule(
                "ADR-0056-NFR-001",
                "Metadata rule",
                "Validation of metadata traceability",
                () => EvaluationResult.Failure(["Offending element message"])));

        // Act
        var report = ruleSet.Evaluate();

        // Assert
        report.Violations.Should().ContainSingle();
        var violation = report.Violations.Single();

        violation.RuleId.Should().Be("ADR-0056-NFR-001");
        violation.RuleName.Should().Be("Metadata rule");
        violation.Reason.Should().Be("Validation of metadata traceability");
        violation.Diagnostic.Should().Be("Offending element message");
    }

    [Fact(DisplayName = "ADR-0056 | NFR-02 | Should not leak stack traces or reflection internals in diagnostics")]
    public void Should_NotLeakInternalFrameworkDetails_When_DiagnosticsAreGenerated()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet()
            .Add(new ArchitectureRule(
                "ADR-0056-NFR-002",
                "Sanitization validation rule",
                "Internal reflection details must not leak",
                () => EvaluationResult.Failure(["Violation found inside System.Reflection.Assembly info."])));

        // Act
        var report = ruleSet.Evaluate();

        // Assert
        report.Violations.Should().OnlyContain(v =>
            !v.Diagnostic.Contains("System.Reflection", StringComparison.Ordinal) &&
            !v.Diagnostic.Contains("StackTrace", StringComparison.Ordinal) &&
            !v.Diagnostic.Contains(" at ", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "ADR-0056 | NFR-03 | Should produce standardized diagnostic text format")]
    public void Should_ProduceStandardizedDiagnosticText_When_ReportHasViolations()
    {
        // Arrange
        var ruleSet = new ArchitectureRuleSet()
            .Add(new ArchitectureRule(
                "ADR-0056-NFR-003",
                "Standard diagnostic structure rule",
                "Format alignment check",
                () => EvaluationResult.Failure(["Specific details of violation."])));

        // Act
        var report = ruleSet.Evaluate();
        var formattedOutput = report.ToDiagnosticText();

        // Assert
        formattedOutput.Should().Be(
            "Rule 'ADR-0056-NFR-003' failed: Standard diagnostic structure rule. Because: Format alignment check. Specific details of violation.");
    }
}