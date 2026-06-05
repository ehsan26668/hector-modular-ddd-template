using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Converters;

internal static class StronglyTypedIdFactory<TId>
    where TId : IStronglyTypedId<TId>
{
    public static TId Create(Guid value)
    {
        return TId.Create(value);
    }
}
