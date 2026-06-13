using System.Text.Json;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class SystemTextJsonOutboxEventSerializer(
    IOutboxEventTypeResolver typeResolver)
    : IOutboxEventSerializer
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public string GetTypeName(INotification notification)
    {
        var metadata = typeResolver.GetMetadata(notification.GetType());
        return metadata.Name;
    }

    public int GetVersion(INotification notification)
    {
        var metadata = typeResolver.GetMetadata(notification.GetType());
        return metadata.Version;
    }

    public string Serialize(INotification notification)
    {
        return JsonSerializer.Serialize(notification, notification.GetType(), Options);
    }

    public INotification Deserialize(OutboxMessage message)
    {
        var type = typeResolver.Resolve(message.Type, message.Version);

        return (INotification)(JsonSerializer.Deserialize(message.Content, type, Options)
            ?? throw new InvalidOperationException($"Failed to deserialize."));
    }
}