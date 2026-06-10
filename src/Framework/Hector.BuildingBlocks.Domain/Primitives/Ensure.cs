using System.Runtime.CompilerServices;

namespace Hector.BuildingBlocks.Domain.Primitives;

public static class Ensure
{
    public static T NotNull<T>(
        T? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
        where T : class
    {
        if (value is null)
        {
            throw new BusinessRuleViolationException(
                message ?? $"'{parameterName}' must not be null.");
        }

        return value;
    }

    public static string NotEmpty(
        string? value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BusinessRuleViolationException(
                message ?? $"'{parameterName}' must not be empty.");
        }

        return value;
    }

    public static T NotDefault<T>(
        T value,
        string? message = null,
        [CallerArgumentExpression(nameof(value))]
        string? parameterName = null)
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
        {
            throw new BusinessRuleViolationException(
                message ?? $"'{parameterName}' must not be default.");
        }

        return value;
    }

    public static void True(
        bool condition,
        string? message = null,
        [CallerArgumentExpression(nameof(condition))]
        string? conditionExpression = null)
    {
        if (!condition)
        {
            throw new BusinessRuleViolationException(
                message ?? $"Condition '{conditionExpression}' must be true.");
        }
    }
}