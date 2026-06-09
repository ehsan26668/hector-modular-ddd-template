using System.Text.Json;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class SystemTextJsonOutboxEventSerializer : IOutboxEventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IOutboxEventTypeResolver _typeResolver;

    public SystemTextJsonOutboxEventSerializer(IOutboxEventTypeResolver typeResolver)
    {
        _typeResolver = typeResolver;
    }

    public INotification Deserialize(OutboxMessage message)
    {
        var type = _typeResolver.Resolve(message.Type);

        if (type is null)
        {
            throw new InvalidOperationException(
                $"Outbox message typr '{message.Type}' could not be resolved.");
        }

        var domainEvent = JsonSerializer.Deserialize(
            message.Content,
            type,
            Options) as INotification;

        if (domainEvent is null)
        {
            throw new InvalidOperationException(
                $"Outbox message {message.Id} deserialized to null.");
        }

        return domainEvent;
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
}