using System.Collections.Concurrent;

namespace Hector.BuildingBlocks.Persistence.Outbox;

public sealed class CachedOutboxEventTypeResolver : IOutboxEventTypeResolver
{
    private static readonly ConcurrentDictionary<string, Type?> Cache = new();

    public Type? Resolve(string typeName)
    {
        return Cache.GetOrAdd(typeName, static name =>
        {
            var type = Type.GetType(name);

            if (type is not null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(name);

                if (type is not null) return type;
            }

            return null;
        });
    }
}
