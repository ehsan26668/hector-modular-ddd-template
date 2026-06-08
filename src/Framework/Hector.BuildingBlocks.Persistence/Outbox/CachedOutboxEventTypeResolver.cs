using System.Collections.Concurrent;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class CachedOutboxEventTypeResolver : IOutboxEventTypeResolver
{
    private static readonly ConcurrentDictionary<string, Type?> Cache = new();

    public Type? Resolve(string typeName)
    {
        return Cache.GetOrAdd(typeName, static name => Type.GetType(name));
    }
}
