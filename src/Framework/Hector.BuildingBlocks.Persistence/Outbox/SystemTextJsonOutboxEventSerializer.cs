using System.Collections.Concurrent;
using System.Text.Json;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class SystemTextJsonOutboxEventSerializer(
    IOutboxEventTypeResolver typeResolver)
    : IOutboxEventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly ConcurrentDictionary<Type, Func<string, INotification>> DeserializerCache = new();

    private readonly IOutboxEventTypeResolver _typeResolver = typeResolver;

    public INotification Deserialize(OutboxMessage message)
    {
        var type = _typeResolver.Resolve(message.Type);

        if (type is null)
        {
            throw new InvalidOperationException(
                $"Outbox message type '{message.Type}' could not be resolved.");
        }

        var deserializer = DeserializerCache.GetOrAdd(type, CreateDeserializer);

        return deserializer(message.Content);
    }

    public string GetTypeName(INotification notification)
    {
        return notification.GetType().AssemblyQualifiedName
            ?? throw new InvalidOperationException(
                $"Event type '{notification.GetType().Name}' does not have an assembly-qualified name.");
    }

    public string Serialize(INotification notification)
    {
        return JsonSerializer.Serialize(
            notification,
            notification.GetType(),
            Options);
    }

    private static Func<string, INotification> CreateDeserializer(Type type)
    {
        return json =>
        {
            var result = JsonSerializer.Deserialize(json, type, Options);

            if (result is not INotification notification)
            {
                throw new InvalidOperationException(
                    $"Failed to deserialize event type '{type.Name}'.");
            }

            return notification;
        };
    }
}
