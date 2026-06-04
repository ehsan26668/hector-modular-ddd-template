namespace Hector.BuildingBlocks.Domain.Primitives;

public static class Ensure
{
    public static void NotNull(object? value, string message)
    {
        if (value is null)
        {
            throw new BusinessRuleViolationException(message);
        }
    }

    public static void NotEmpty(string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleViolationException(message);
        }
    }

    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new BusinessRuleViolationException(message);
        }
    }
}