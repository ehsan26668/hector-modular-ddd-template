using FluentValidation;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.Messaging;

internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorList = validators as IValidator<TRequest>[] ?? [.. validators];

        if (validatorList.Length == 0)
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var failures = new Dictionary<string, List<string>>();

        foreach (var validator in validatorList)
        {
            var validationResult = await validator.ValidateAsync(context, cancellationToken);

            if (validationResult.IsValid)
            {
                continue;
            }

            foreach (var validationFailure in validationResult.Errors)
            {
                if (!failures.TryGetValue(validationFailure.PropertyName, out var list))
                {
                    list = [];
                    failures[validationFailure.PropertyName] = list;
                }

                list.Add(validationFailure.ErrorMessage);
            }
        }

        if (failures.Count == 0)
        {
            return await next();
        }

        var failureDictionary = failures.ToDictionary(
            failure => failure.Key,
            failure => failure.Value.ToArray());

        var validationError = ValidationError.Create(
            "validation.failed",
            "Validation failed",
            failureDictionary);

        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(validationError);
        }

        if (responseType.IsGenericType &&
            responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var failureMethod = responseType.GetMethod(nameof(Result<object>.Failure));

            return (TResponse)failureMethod!.Invoke(
                null,
                [validationError])!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior can only be used with Result or Result<T>. Response type: {responseType.Name}");
    }
}
