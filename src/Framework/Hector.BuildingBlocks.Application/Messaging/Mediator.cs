using System.Collections.Concurrent;
using System.Linq.Expressions;
using Hector.BuildingBlocks.Domain.Primitives;
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

        var handlerMetadata = _handlerMetadataCache.GetOrAdd(
            (requestType, responseType),
            static t => CreateHandlerMetadata(t.request, t.response));

        var handlerInstance = _serviceProvider.GetRequiredService(handlerMetadata.HandlerType);

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);

        var behaviors = _serviceProvider.GetServices(behaviorType);

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
        {
            return (Task<TResponse>)handlerMetadata.Invoker(
                handlerInstance,
                request,
                cancellationToken);
        };

        if (behaviors is ICollection<object> collection)
        {
            var array = new object[collection.Count];
            collection.CopyTo(array, 0);

            for (int i = array.Length - 1; i >= 0; i--)
            {
                handlerDelegate = WrapBehavior(
                    array[i]!, // Fixed CS8604
                    handlerDelegate,
                    request,
                    cancellationToken);
            }
        }
        else
        {
            // Fallback for non-collection enumerables (less efficient but safe)
            var behaviorList = behaviors.Reverse().ToList();
            foreach (var behavior in behaviorList)
            {
                handlerDelegate = WrapBehavior(
                    behavior!,
                    handlerDelegate,
                    request,
                    cancellationToken);
            }
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

        var handlers = _serviceProvider.GetServices(handlerMetadata.HandlerType);

        foreach (var handler in handlers)
        {
            var task = (Task)handlerMetadata.Invoker(
                handler!,
                notification!,
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

        // Fixed syntax error here: added 't =>'
        var behaviorInvoker = _behaviorInvokerCache.GetOrAdd(
            behaviorType,
            static t => CompileBehaviorInvoker(t));

        return () =>
        {
            return (Task<TResponse>)behaviorInvoker(
                behavior,
                request,
                next,
                cancellationToken);
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

        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var requestParam = Expression.Parameter(typeof(object), "request");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        var handlerCast = Expression.Convert(handlerParam, handlerType);
        var requestCast = Expression.Convert(requestParam, requestType);

        var call = Expression.Call(handlerCast, method, requestCast, ctParam);
        var resultCast = Expression.Convert(call, typeof(object));

        return Expression
            .Lambda<HandlerInvoker>(resultCast, handlerParam, requestParam, ctParam)
            .Compile();
    }

    private static NotificationInvoker CompileNotificationInvoker(Type handlerType, Type notificationType)
    {
        var method = handlerType.GetMethod(HandleAsyncMethodName)!;

        var handlerParam = Expression.Parameter(typeof(object), "handler");
        var notificationParam = Expression.Parameter(typeof(object), "notification");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        var handlerCast = Expression.Convert(handlerParam, handlerType);
        var notificationCast = Expression.Convert(notificationParam, notificationType);

        var call = Expression.Call(handlerCast, method, notificationCast, ctParam);
        var resultCast = Expression.Convert(call, typeof(object));

        return Expression
            .Lambda<NotificationInvoker>(resultCast, handlerParam, notificationParam, ctParam)
            .Compile();
    }

    private static BehaviorInvoker CompileBehaviorInvoker(Type behaviorType)
    {
        var method = behaviorType.GetMethod(HandleAsyncMethodName)!;
        var parameters = method.GetParameters();

        var requestType = parameters[0].ParameterType;
        var nextType = parameters[1].ParameterType;

        var behaviorParam = Expression.Parameter(typeof(object), "behavior");
        var requestParam = Expression.Parameter(typeof(object), "request");
        var nextParam = Expression.Parameter(typeof(object), "next");
        var ctParam = Expression.Parameter(typeof(CancellationToken), "ct");

        var behaviorCast = Expression.Convert(behaviorParam, behaviorType);
        var requestCast = Expression.Convert(requestParam, requestType);
        var nextCast = Expression.Convert(nextParam, nextType);

        var call = Expression.Call(behaviorCast, method, requestCast, nextCast, ctParam);
        var resultCast = Expression.Convert(call, typeof(object));

        return Expression
            .Lambda<BehaviorInvoker>(resultCast, behaviorParam, requestParam, nextParam, ctParam)
            .Compile();
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
