using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Application.Messaging;

public static class MediatorServiceCollectionExtensions
{
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddScoped<IMediator, Mediator>();

        RegisterRequestHandlers(services, assembly);
        RegisterNotificationHandlers(services, assembly);

        return services;
    }

    private static void RegisterRequestHandlers(
        IServiceCollection services,
        Assembly assembly)
    {
        var handlerType = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Handler = t, Service = i }));

        foreach (var handler in handlerType)
        {
            services.AddScoped(handler.Service, handler.Handler);
        }
    }

    private static void RegisterNotificationHandlers(
        IServiceCollection services,
        Assembly assembly)
    {
        var handlerTypes = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(INotificationHandler<>))
                .Select(i => new { Handler = t, Service = i }));

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Service, handler.Handler);
        }
    }
}