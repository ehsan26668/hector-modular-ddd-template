using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Messaging;

internal sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> SendAsync<TResponse>(
    IRequest<TResponse> request,
    CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var handlerSpecificType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, responseType);

        var handler = _serviceProvider.GetRequiredService(handlerSpecificType);

        RequestHandlerDelegate<TResponse> requestHandlerDelegate = () =>
        {
            var handlerMethodInfo = handlerSpecificType.GetMethod(
                nameof(IRequestHandler<IRequest<TResponse>, TResponse>.HandleAsync));

            if (handlerMethodInfo is null)
            {
                throw new InvalidOperationException(
                    $"Could not find HandleAsync method on handler type {handlerSpecificType.FullName}");
            }

            var result = handlerMethodInfo.Invoke(handler, [request, cancellationToken]);

            if (result is null)
            {
                throw new InvalidOperationException(
                    $"Handler for request type {requestType.Name} returned null.");
            }

            return (Task<TResponse>)result;
        };

        var pipelineSpecificType = typeof(IPipelineBehavior<,>)
            .MakeGenericType(requestType, responseType);

        var behaviors = _serviceProvider
            .GetServices(pipelineSpecificType)
            .Where(behavior => behavior is not null)
            .Reverse();

        RequestHandlerDelegate<TResponse> currentDelegate = requestHandlerDelegate;

        foreach (var behavior in behaviors)
        {
            var behaviorInstance = behavior
                ?? throw new InvalidOperationException(
                    $"Resolved null pipeline behavior for request type {requestType.Name}.");

            var behaviorMethodInfo = behaviorInstance
                .GetType()
                .GetMethod(nameof(IPipelineBehavior<IRequest<TResponse>, TResponse>.HandleAsync));

            if (behaviorMethodInfo is null)
            {
                throw new InvalidOperationException(
                    $"Could not find HandleAsync method on pipeline behavior type {behaviorInstance.GetType().FullName}");
            }

            var nextDelegate = currentDelegate;

            currentDelegate = () =>
            {
                var result = behaviorMethodInfo.Invoke(
                    behaviorInstance,
                    [request, nextDelegate, cancellationToken]);

                if (result is null)
                {
                    throw new InvalidOperationException(
                        $"Pipeline behavior for request type {requestType.Name} returned null.");
                }

                return (Task<TResponse>)result;
            };
        }

        return await currentDelegate();
    }
}
