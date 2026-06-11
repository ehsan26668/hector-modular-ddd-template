using FluentValidation;

namespace Hector.BuildingBlocks.Application.Messaging;

internal sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IValidator<TRequest>[] _validators = validators?.ToArray()
        ?? throw new ArgumentNullException(nameof(validators));

    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(next);

        if (_validators.Length == 0)
        {
            return await next().ConfigureAwait(false);
        }

        var results = await Task.WhenAll(
                _validators.Select(validator =>
                    validator.ValidateAsync(
                        new ValidationContext<TRequest>(request),
                        cancellationToken)))
            .ConfigureAwait(false);

        var failures = results
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        return await next().ConfigureAwait(false);
    }
}
