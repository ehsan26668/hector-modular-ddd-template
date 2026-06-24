using System.Collections.Concurrent;
using System.Linq.Expressions;
using Hector.BuildingBlocks.Domain.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Messaging;

internal sealed class Mediator(IServiceProvider serviceProvider) : IMediator
{
    private const string HandleMethodName = "Handle";
    private const string HandleAsyncMethodName = "HandleAsync";

    private static readonly ConcurrentDictionary<(Type RequestType, Type ResponseType), HandlerMetadata> HandlerMetadataCache = new();
    private static readonly ConcurrentDictionary<Type, NotificationHandlerMetadata> NotificationHandlerMetadataCache = new();
    private static readonly ConcurrentDictionary<Type, BehaviorInvoker> BehaviorInvokerCache = new();

    private readonly IServiceProvider _serviceProvider = serviceProvider
        ?? throw new ArgumentNullException(nameof(serviceProvider));

    public async Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var requestType = request.GetType();
        var responseType = typeof(TResponse);

        var handlerMetadata = HandlerMetadataCache.GetOrAdd(
            (requestType, responseType),
            static key => CreateHandlerMetadata(key.RequestType, key.ResponseType));

        var handler = _serviceProvider.GetRequiredService(handlerMetadata.HandlerType);

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
            (Task<TResponse>)handlerMetadata.Invoker(handler, request, cancellationToken);

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);
        var behaviors = _serviceProvider
            .GetServices(behaviorType)
            .ToArray();

        for (var index = behaviors.Length - 1; index >= 0; index--)
        {
            handlerDelegate = WrapBehavior(
                behaviors[index]!,
                handlerDelegate,
                request,
                cancellationToken);
        }

        return await handlerDelegate().ConfigureAwait(false);
    }

    public async Task PublishAsync<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);
        cancellationToken.ThrowIfCancellationRequested();

        var notificationType = notification.GetType();

        var handlerMetadata = NotificationHandlerMetadataCache.GetOrAdd(
            notificationType,
            static type => CreateNotificationHandlerMetadata(type));

        var handlers = _serviceProvider
            .GetServices(handlerMetadata.HandlerType)
            .ToArray();

        foreach (var handler in handlers)
        {
            var handlingTask = (Task)handlerMetadata.Invoker(
                handler!,
                notification,
                cancellationToken);

            await handlingTask.ConfigureAwait(false);
        }
    }

    private static RequestHandlerDelegate<TResponse> WrapBehavior<TResponse>(
        object behavior,
        RequestHandlerDelegate<TResponse> next,
        object request,
        CancellationToken cancellationToken)
    {
        var behaviorInvoker = BehaviorInvokerCache.GetOrAdd(
            behavior.GetType(),
            static behaviorType => CompileBehaviorInvoker(behaviorType));

        return () => (Task<TResponse>)behaviorInvoker(
            behavior,
            request,
            next,
            cancellationToken);
    }

    private static HandlerMetadata CreateHandlerMetadata(Type requestType, Type responseType)
    {
        var handlerType = ResolveHandlerType(requestType, responseType);
        var invoker = CompileHandlerInvoker(handlerType, requestType);

        return new HandlerMetadata(handlerType, invoker);
    }

    private static Type ResolveHandlerType(Type requestType, Type responseType)
    {
        var commandInterface = requestType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        if (commandInterface is not null)
        {
            var valueType = commandInterface.GetGenericArguments()[0];
            return typeof(ICommandHandler<,>).MakeGenericType(requestType, valueType);
        }

        var queryInterface = requestType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>));

        if (queryInterface is not null)
        {
            var valueType = queryInterface.GetGenericArguments()[0];
            return typeof(IQueryHandler<,>).MakeGenericType(requestType, valueType);
        }

        return typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
    }

    private static NotificationHandlerMetadata CreateNotificationHandlerMetadata(Type notificationType)
    {
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);
        var invoker = CompileNotificationInvoker(handlerType, notificationType);

        return new NotificationHandlerMetadata(handlerType, invoker);
    }

    private static HandlerInvoker CompileHandlerInvoker(Type handlerType, Type requestType)
    {
        var allInterfaces = new List<Type> { handlerType };
        allInterfaces.AddRange(handlerType.GetInterfaces());

        var method = allInterfaces
            .SelectMany(i => i.GetMethods())
            .FirstOrDefault(m => m.Name == HandleMethodName &&
                                 m.GetParameters().Length == 2 &&
                                 m.GetParameters()[0].ParameterType == requestType);

        if (method is null)
        {
            throw new InvalidOperationException($"Method '{HandleMethodName}' with request type '{requestType.Name}' not found on '{handlerType.FullName}'.");
        }

        var handlerParameter = Expression.Parameter(typeof(object), "handler");
        var requestParameter = Expression.Parameter(typeof(object), "request");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var handlerCast = Expression.Convert(handlerParameter, handlerType);
        var requestCast = Expression.Convert(requestParameter, requestType);

        var methodCall = Expression.Call(handlerCast, method, requestCast, cancellationTokenParameter);
        var castResult = Expression.Convert(methodCall, typeof(object));

        return Expression.Lambda<HandlerInvoker>(castResult, handlerParameter, requestParameter, cancellationTokenParameter).Compile();
    }

    private static NotificationInvoker CompileNotificationInvoker(Type handlerType, Type notificationType)
    {
        // 1. جستجوی متد با نام و امضای دقیق (HandleAsync(TNotification, CancellationToken))
        var method = handlerType.GetMethods().FirstOrDefault(m =>
            m.Name == HandleAsyncMethodName &&
            m.GetParameters().Length == 2 &&
            m.GetParameters()[0].ParameterType == notificationType &&
            m.GetParameters()[1].ParameterType == typeof(CancellationToken));

        if (method is null)
        {
            throw new InvalidOperationException($"Method '{HandleAsyncMethodName}' with notification type '{notificationType.Name}' and CancellationToken not found on '{handlerType.FullName}'.");
        }

        var handlerParameter = Expression.Parameter(typeof(object), "handler");
        var notificationParameter = Expression.Parameter(typeof(object), "notification");
        var cancellationTokenParameter = Expression.Parameter(typeof(CancellationToken), "cancellationToken");

        var handlerCast = Expression.Convert(handlerParameter, handlerType);
        var notificationCast = Expression.Convert(notificationParameter, notificationType);

        var methodCall = Expression.Call(handlerCast, method, notificationCast, cancellationTokenParameter);
        var castResult = Expression.Convert(methodCall, typeof(object));

        return Expression.Lambda<NotificationInvoker>(castResult, handlerParameter, notificationParameter, cancellationTokenParameter).Compile();
    }

    private static BehaviorInvoker CompileBehaviorInvoker(Type behaviorType)
    {
        var method = behaviorType.GetMethods().FirstOrDefault(m => m.Name == HandleMethodName)
            ?? throw new InvalidOperationException($"Method '{HandleMethodName}' not found on behavior '{behaviorType.FullName}'.");

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

        var methodCall = Expression.Call(behaviorCast, method, requestCast, nextCast, cancellationTokenParameter);
        var castResult = Expression.Convert(methodCall, typeof(object));

        return Expression.Lambda<BehaviorInvoker>(castResult, behaviorParameter, requestParameter, nextParameter, cancellationTokenParameter).Compile();
    }

    private sealed class HandlerMetadata(Type handlerType, HandlerInvoker invoker) { public Type HandlerType { get; } = handlerType; public HandlerInvoker Invoker { get; } = invoker; }
    private sealed class NotificationHandlerMetadata(Type handlerType, NotificationInvoker invoker) { public Type HandlerType { get; } = handlerType; public NotificationInvoker Invoker { get; } = invoker; }

    private delegate object HandlerInvoker(object handler, object request, CancellationToken cancellationToken);
    private delegate object NotificationInvoker(object handler, object notification, CancellationToken cancellationToken);
    private delegate object BehaviorInvoker(object behavior, object request, object next, CancellationToken cancellationToken);
}
