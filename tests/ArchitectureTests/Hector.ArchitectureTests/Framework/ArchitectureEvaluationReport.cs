using System.Text;

namespace Hector.ArchitectureTests.Framework;

public sealed class ArchitectureEvaluationReport(
    IReadOnlyList<ArchitectureViolation> violations)
{
    public IReadOnlyList<ArchitectureViolation> Violations { get; } = [.. violations
            .OrderBy(v => v.RuleId, StringComparer.Ordinal)
            .ThenBy(v => v.Diagnostic, StringComparer.Ordinal)];
    public bool HasViolations => Violations.Count > 0;

    public string ToDiagnosticText()
    {
        if (!HasViolations)
        {
            return "Architecture evaluation succeeded.";
        }

        var builder = new StringBuilder();
        foreach (var violation in Violations)
        {
            builder.AppendLine($"Rule '{violation.RuleId}' failed: {violation.RuleName}. Because: {violation.Reason}. {violation.Diagnostic}");
        }

        return builder.ToString().TrimEnd();
    }
}