using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Messaging;

internal sealed class Mediator : IMediator
{
    private const string HandleAsyncMethodName = "HandleAsync";

    private readonly IServiceProvider _serviceProvider;

    private static readonly ConcurrentDictionary<(Type request, Type response), Type> _handlerTypeCache = new();
    private static readonly ConcurrentDictionary<(Type request, Type response), MethodInfo> _handlerMethodCache = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo> _behaviorMethodCache = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var responseType = typeof(TResponse);
        var key = (requestType, responseType);

        // ✅ 1. Cache handler type
        var handlerType = _handlerTypeCache.GetOrAdd(key, static t =>
            typeof(IRequestHandler<,>).MakeGenericType(t.request, t.response));

        // ✅ 2. Resolve handler instance
        var handlerInstance = _serviceProvider.GetRequiredService(handlerType);

        // ✅ 3. Cache HandleAsync method
        var handleMethod = _handlerMethodCache.GetOrAdd(key, static t =>
            typeof(IRequestHandler<,>)
                .MakeGenericType(t.request, t.response)
                .GetMethod(HandleAsyncMethodName)!);

        // ✅ 4. Resolve behaviors
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);

        var behaviors = _serviceProvider
            .GetServices(behaviorType)
            .Cast<object>()
            .ToList();

        // ✅ 5. Core handler delegate
        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            var task = (Task<TResponse>)handleMethod.Invoke(
                handlerInstance, new object[] { request, cancellationToken })!;

            return task;
        };

        // ✅ 6. Build pipeline chain (reverse order)
        foreach (var behavior in Enumerable.Reverse(behaviors))
        {
            handlerDelegate = WrapBehavior(behavior, handlerDelegate, request, cancellationToken);
        }

        return await handlerDelegate();
    }

    private static RequestHandlerDelegate<TResponse> WrapBehavior<TResponse>(
        object behavior,
        RequestHandlerDelegate<TResponse> next,
        object request,
        CancellationToken cancellationToken)
    {
        var behaviorType = behavior.GetType();

        var method = _behaviorMethodCache.GetOrAdd(behaviorType, static t =>
            t.GetMethod(HandleAsyncMethodName)!);

        return () =>
        {
            var task = (Task<TResponse>)method.Invoke(
                behavior,
                new object[] { request, next, cancellationToken })!;

            return task;
        };
    }
}
