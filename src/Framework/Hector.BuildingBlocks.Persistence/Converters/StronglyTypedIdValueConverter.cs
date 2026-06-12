using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Converters;

public class StronglyTypedIdValueConverter<TId> : ValueConverter<TId, Guid>
    where TId : StronglyTypedId<TId>
{
    private static readonly ConcurrentDictionary<Type, Func<Guid, TId>> _factories = new();

    public StronglyTypedIdValueConverter() : base(
        id => id.Value,
        guid => Create(guid))
    {
    }

    private static TId Create(Guid guid)
    {
        var factory = _factories.GetOrAdd(typeof(TId), type =>
        {
            var ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                [typeof(Guid)],
                null) ?? throw new InvalidOperationException(
                    $"Type {type.Name} must have a private constructor taking a Guid.");
            return g => (TId)ctor.Invoke(new object[] { g });
        });

        return factory(guid);
    }
}
