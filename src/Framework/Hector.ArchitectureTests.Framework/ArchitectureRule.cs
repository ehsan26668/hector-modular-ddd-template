namespace Hector.ArchitectureTests.Framework;

public partial class ArchitectureRule
{
    public string Id { get; }
    public string Name { get; }
    public string Reason { get; private set; }

    private readonly Action? _assertion;
    private readonly Func<EvaluationResult>? _evaluator;

    public ArchitectureRule(
        string id,
        string name,
        string because,
        Action assertion)
    {
        Id = id;
        Name = name;
        Reason = because;
        _assertion = assertion;
    }

    public ArchitectureRule(
        string id,
        string name,
        string because,
        Func<EvaluationResult> evaluator)
    {
        Id = id;
        Name = name;
        Reason = because;
        _evaluator = evaluator;
    }

    public ArchitectureRule Because(string reason)
    {
        Reason = reason;
        return this;
    }

    public void Evaluate()
    {
        if (_assertion is not null)
        {
            _assertion();
            return;
        }

        if (_evaluator is null)
        {
            return;
        }

        var result = _evaluator();

        if (!result.HasViolations)
        {
            return;
        }

        throw new InvalidOperationException(
            $"Rule '{Id}' failed: {Name}. Because: {Reason}.{Environment.NewLine}" +
            string.Join(Environment.NewLine, result.Diagnostics));
    }

    public EvaluationResult EvaluateWithResult()
    {
        if (_evaluator is not null)
        {
            return _evaluator();
        }

        try
        {
            _assertion?.Invoke();
            return EvaluationResult.Success();
        }
        catch (Exception exception)
        {
            return EvaluationResult.Failure(new List<string> { exception.Message });
        }
    }
}
