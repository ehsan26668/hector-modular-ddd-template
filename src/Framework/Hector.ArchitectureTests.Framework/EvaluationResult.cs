namespace Hector.ArchitectureTests.Framework;

public sealed class EvaluationResult(IReadOnlyList<string> diagnostics)
{
    public bool HasViolations => Diagnostics.Count > 0;
    public IReadOnlyList<string> Diagnostics { get; } = diagnostics;

    public static EvaluationResult Success() => new([]);
    public static EvaluationResult Failure(IReadOnlyList<string> violations) => new(violations);
}