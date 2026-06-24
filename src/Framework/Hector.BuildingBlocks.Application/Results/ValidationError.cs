namespace Hector.BuildingBlocks.Application.Results;

public static class ValidationError
{
    public static Error Create(
        string code,
        string message,
        IReadOnlyDictionary<string, string[]> failures)
    {
        return new Error(
            code,
            message,
            ErrorCategory.Validation,
            new Dictionary<string, object>
            {
                ["failures"] = failures
            });
    }
}