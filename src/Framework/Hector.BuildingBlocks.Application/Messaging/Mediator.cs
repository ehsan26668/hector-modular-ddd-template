using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Messaging;

internal sealed class Mediator : IMediator
{
    private const string HandleAsyncMethodName = "HandleAsync";

    private readonly IServiceProvider _serviceProvider;

    private static readonly ConcurrentDictionary<(Type request, Type response), Type> _handlerTypeCache = new();

    private static readonly ConcurrentDictionary<(Type request, Type response), HandlerInvoker> _handlerInvokerCache = new();

    private static readonly ConcurrentDictionary<Type, BehaviorInvoker> _behaviorInvokerCache = new();

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

        var handlerType = _handlerTypeCache.GetOrAdd(key, static t =>
            typeof(IRequestHandler<,>).MakeGenericType(t.request, t.response));

        var handlerInstance = _serviceProvider.GetRequiredService(handlerType);

        var handlerInvoker = _handlerInvokerCache.GetOrAdd(key, static t =>
            CompileHandlerInvoker(t.request, t.response));

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);

        var behaviors = _serviceProvider
            .GetServices(behaviorType)
            .Cast<object>()
            .ToList();

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            var task = (Task<TResponse>)handlerInvoker(
                handlerInstance,
                request,
                cancellationToken);

            return task;
        };

        foreach (var behavior in Enumerable.Reverse(behaviors))
        {
            handlerDelegate = WrapBehavior(
                behavior,
                handlerDelegate,
                request,
                cancellationToken);
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

        var behaviorInvoker = _behaviorInvokerCache.GetOrAdd(
            behaviorType,
            static t => CompileBehaviorInvoker(t));

        return () =>
        {
            var task = (Task<TResponse>)behaviorInvoker(
                behavior,
                request,
                next,
                cancellationToken);

            return task;
        };
    }

    private static HandlerInvoker CompileHandlerInvoker(
        Type requestType,
        Type responseType)
    {
        var handlerInterfaceType = typeof(IRequestHandler<,>)
            .MakeGenericType(requestType, responseType);

        var method = handlerInterfaceType.GetMethod(HandleAsyncMethodName)!;

        var handlerParameter = Expression.Parameter(typeof(object), "handler");
        var requestParameter = Expression.Parameter(typeof(object), "request");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var handlerCast = Expression.Convert(handlerParameter, handlerInterfaceType);
        var requestCast = Expression.Convert(requestParameter, requestType);

        var call = Expression.Call(
            handlerCast,
            method,
            requestCast,
            cancellationTokenParameter);

        var resultCast = Expression.Convert(call, typeof(object));

        return Expression
            .Lambda<HandlerInvoker>(
                resultCast,
                handlerParameter,
                requestParameter,
                cancellationTokenParameter)
            .Compile();
    }

    private static BehaviorInvoker CompileBehaviorInvoker(Type behaviorType)
    {
        var method = behaviorType.GetMethod(HandleAsyncMethodName)!;

        var parameters = method.GetParameters();

        var requestType = parameters[0].ParameterType;
        var nextType = parameters[1].ParameterType;

        var behaviorParameter = Expression.Parameter(typeof(object), "behavior");
        var requestParameter = Expression.Parameter(typeof(object), "request");
        var nextParameter = Expression.Parameter(typeof(object), "next");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var behaviorCast = Expression.Convert(behaviorParameter, behaviorType);
        var requestCast = Expression.Convert(requestParameter, requestType);
        var nextCast = Expression.Convert(nextParameter, nextType);

        var call = Expression.Call(
            behaviorCast,
            method,
            requestCast,
            nextCast,
            cancellationTokenParameter);

        var resultCast = Expression.Convert(call, typeof(object));

        return Expression
            .Lambda<BehaviorInvoker>(
                resultCast,
                behaviorParameter,
                requestParameter,
                nextParameter,
                cancellationTokenParameter)
            .Compile();
    }

    private delegate object HandlerInvoker(
        object handler,
        object request,
        CancellationToken cancellationToken);

    private delegate object BehaviorInvoker(
        object behavior,
        object request,
        object next,
        CancellationToken cancellationToken);
}
