namespace Hector.BuildingBlocks.Domain.Primitives;

public interface IStronglyTypedId<TSelf>
{
    static abstract TSelf Create(Guid value);

    static abstract TSelf CreateEmpty();
}