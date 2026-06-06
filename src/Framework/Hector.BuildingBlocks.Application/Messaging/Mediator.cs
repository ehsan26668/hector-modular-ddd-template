using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Messaging;

internal sealed class Mediator : IMediator
{
    private const string HandleAsyncMethodName = "HandleAsync";

    private readonly IServiceProvider _serviceProvider;

    private static readonly ConcurrentDictionary<(Type request, Type response), HandlerMetadata> _handlerMetadataCache = new();
    private static readonly ConcurrentDictionary<Type, NotificationHandlerMetadata> _notificationHandlerMetadataCache = new();
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

        var handlerMetadata = _handlerMetadataCache.GetOrAdd(
            key,
            static t => CreateHandlerMetadata(t.request, t.response));

        var handlerInstance = _serviceProvider.GetRequiredService(handlerMetadata.HandlerType);

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);

        var behaviors = _serviceProvider
            .GetServices(behaviorType)
            .Cast<object>()
            .ToList();

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            var task = (Task<TResponse>)handlerMetadata.Invoker(
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

    public async Task PublishAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        var notificationType = notification.GetType();

        var handlerMetadata = _notificationHandlerMetadataCache.GetOrAdd(
            notificationType,
            static t => CreateNotificationHandlerMetadata(t));

        var handlerInstances = _serviceProvider
            .GetServices(handlerMetadata.HandlerType)
            .Cast<object>()
            .ToList();

        foreach (var handler in handlerInstances)
        {
            var task = (Task)handlerMetadata.Invoker(
                handler,
                notification,
                cancellationToken);

            await task;
        }
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

    private static HandlerMetadata CreateHandlerMetadata(Type requestType, Type responseType)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
        var invoker = CompileHandlerInvoker(handlerType, requestType);
        return new HandlerMetadata(handlerType, invoker);
    }

    private static NotificationHandlerMetadata CreateNotificationHandlerMetadata(Type notificationType)
    {
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var invoker = CompileNotificationInvoker(handlerType, notificationType);
        return new NotificationHandlerMetadata(handlerType, invoker);
    }

    private static HandlerInvoker CompileHandlerInvoker(Type handlerType, Type requestType)
    {
        var method = handlerType.GetMethod(HandleAsyncMethodName)!;

        var handlerParameter = Expression.Parameter(typeof(object), "handler");
        var requestParameter = Expression.Parameter(typeof(object), "request");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var handlerCast = Expression.Convert(handlerParameter, handlerType);
        var requestCast = Expression.Convert(requestParameter, requestType);

        var call = Expression.Call(handlerCast, method, requestCast, cancellationTokenParameter);
        var resultCast = Expression.Convert(call, typeof(object));

        return Expression.Lambda<HandlerInvoker>(resultCast, handlerParameter, requestParameter, cancellationTokenParameter).Compile();
    }

    private static NotificationInvoker CompileNotificationInvoker(Type handlerType, Type notificationType)
    {
        var method = handlerType.GetMethod(HandleAsyncMethodName)!;

        var handlerParameter = Expression.Parameter(typeof(object), "handler");
        var notificationParameter = Expression.Parameter(typeof(object), "notification");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var handlerCast = Expression.Convert(handlerParameter, handlerType);
        var notificationCast = Expression.Convert(notificationParameter, notificationType);

        var call = Expression.Call(handlerCast, method, notificationCast, cancellationTokenParameter);
        var resultCast = Expression.Convert(call, typeof(object));

        return Expression.Lambda<NotificationInvoker>(resultCast, handlerParameter, notificationParameter, cancellationTokenParameter).Compile();
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

        var call = Expression.Call(behaviorCast, method, requestCast, nextCast, cancellationTokenParameter);
        var resultCast = Expression.Convert(call, typeof(object));

        return Expression.Lambda<BehaviorInvoker>(resultCast, behaviorParameter, requestParameter, nextParameter, cancellationTokenParameter).Compile();
    }

    private sealed class HandlerMetadata
    {
        public HandlerMetadata(Type handlerType, HandlerInvoker invoker)
        {
            HandlerType = handlerType;
            Invoker = invoker;
        }
        public Type HandlerType { get; }
        public HandlerInvoker Invoker { get; }
    }

    private sealed class NotificationHandlerMetadata
    {
        public NotificationHandlerMetadata(Type handlerType, NotificationInvoker invoker)
        {
            HandlerType = handlerType;
            Invoker = invoker;
        }
        public Type HandlerType { get; }
        public NotificationInvoker Invoker { get; }
    }

    private delegate object HandlerInvoker(object handler, object request, CancellationToken cancellationToken);
    private delegate object NotificationInvoker(object handler, object notification, CancellationToken cancellationToken);
    private delegate object BehaviorInvoker(object behavior, object request, object next, CancellationToken cancellationToken);
}
