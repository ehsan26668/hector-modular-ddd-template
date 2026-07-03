namespace Hector.ArchitectureTests.Framework;

public sealed record ArchitectureViolation(
    string RuleId,
    string RuleName,
    string Reason,
    string Diagnostic);