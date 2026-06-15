using System.Reflection;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class AttributedOutboxEventTypeResolver : IOutboxEventTypeResolver
{
    private readonly IReadOnlyDictionary<(string Name, int Version), Type> _typeByContract;
    private readonly IReadOnlyDictionary<Type, OutboxEventMetadata> _metadataByType;

    public AttributedOutboxEventTypeResolver(IEnumerable<Assembly> assemblies)
    {
        var typeByContract = new Dictionary<(string Name, int Version), Type>();
        var metadataByType = new Dictionary<Type, OutboxEventMetadata>();

        var eventTypes = assemblies
            .Where(assembly => assembly is not null)
            .GroupBy(assembly => assembly.FullName)
            .Select(group => group.First())
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                !type.IsAbstract &&
                !type.IsInterface &&
                typeof(INotification).IsAssignableFrom(type))
            .Distinct();

        foreach (var eventType in eventTypes)
        {
            var attribute = eventType.GetCustomAttribute<OutboxEventAttribute>();

            if (attribute is null)
            {
                continue;
            }

            var metadata = new OutboxEventMetadata(
                attribute.Name,
                attribute.Version,
                eventType);

            var key = (metadata.Name, metadata.Version);

            if (typeByContract.TryGetValue(key, out var existingType))
            {
                if (existingType != eventType)
                {
                    throw new InvalidOperationException(
                        $"Duplicate outbox event contract '{metadata.Name}' with version '{metadata.Version}' " +
                        $"for event types '{existingType.FullName}' and '{eventType.FullName}'.");
                }

                metadataByType.TryAdd(eventType, metadata);
                continue;
            }

            typeByContract.Add(key, eventType);
            metadataByType.TryAdd(eventType, metadata);
        }

        _typeByContract = typeByContract;
        _metadataByType = metadataByType;
    }

    public Type Resolve(string eventName, int version)
    {
        if (_typeByContract.TryGetValue((eventName, version), out var eventType))
        {
            return eventType;
        }

        throw new InvalidOperationException(
            $"Outbox event contract '{eventName}' with version '{version}' could not be resolved.");
    }

    public OutboxEventMetadata GetMetadata(Type eventType)
    {
        if (_metadataByType.TryGetValue(eventType, out var metadata))
        {
            return metadata;
        }

        var attribute = eventType.GetCustomAttribute<OutboxEventAttribute>();

        if (attribute is not null)
        {
            return new OutboxEventMetadata(
                attribute.Name,
                attribute.Version,
                eventType);
        }

        throw new InvalidOperationException(
            $"Outbox event type '{eventType.FullName}' does not define outbox metadata.");
    }
}
