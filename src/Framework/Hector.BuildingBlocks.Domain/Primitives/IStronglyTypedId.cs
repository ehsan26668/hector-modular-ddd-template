namespace Hector.BuildingBlocks.Domain.Primitives;

public interface IStronglyTypedId { }

public interface IStronglyTypedId<TSelf> : IStronglyTypedId
    where TSelf : IStronglyTypedId<TSelf>
{
    static abstract TSelf Create(Guid value);

    static abstract TSelf CreateEmpty();

     Guid Value { get; }
}