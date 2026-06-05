using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Persistence.Converters;

public class StronglyTypedIdEfValueConverter<TId> : ValueConverter<TId, Guid>
    where TId : IStronglyTypedId<TId>
{
    public StronglyTypedIdEfValueConverter() : base(
        id => id.Value,
        value => StronglyTypedIdFactory<TId>.Create(value))
    {
    }
}
