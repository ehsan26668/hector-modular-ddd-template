namespace Hector.BuildingBlocks.Application.Results;

public static class Errors
{
    public static class Validation
    {
        public static Error Failed(string code, string message)
            => new(code, message, ErrorCategory.Validation);
    }

    public static class NotFound
    {
        public static Error Resource(string resourceName, object id)
            => new(
                $"{resourceName}.NotFound",
                $"{resourceName} with id '{id}' was not found.",
                ErrorCategory.NotFound);
    }

    public static class Conflict
    {
        public static Error Resource(string code, string message)
            => new(code, message, ErrorCategory.Conflict);
    }

    public static class BusinessRule
    {
        public static Error Violation(string code, string message)
            => new(code, message, ErrorCategory.BusinessRule);
    }

    public static class Infrastructure
    {
        public static Error Failure(string code, string message)
            => new(code, message, ErrorCategory.Infrastructure);
    }

    public static class Unexpected
    {
        public static Error Exception(Exception exception)
            => new(
                "Unexpected.Error",
                exception.Message,
                ErrorCategory.Unexpected);
    }
}