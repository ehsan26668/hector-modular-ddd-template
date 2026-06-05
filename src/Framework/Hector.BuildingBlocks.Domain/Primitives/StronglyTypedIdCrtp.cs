namespace Hector.BuildingBlocks.Domain.Primitives;

public abstract class StronglyTypedIdCrtp<TSelf> : ValueObject
    where TSelf : StronglyTypedIdCrtp<TSelf>, IStronglyTypedId<TSelf>
{
    public Guid Value { get; }

    protected StronglyTypedIdCrtp(Guid value)
    {
        Value = value;
    }

    public static TSelf New() => TSelf.Create(Guid.CreateVersion7());

    public static TSelf Empty() => TSelf.CreateEmpty();

    public static TSelf Parse(string value)
    {
        var guid = Guid.Parse(value);
        return TSelf.Create(guid);
    }

    public static bool TryParse(string value, out TSelf? id)
    {
        if (Guid.TryParse(value, out var guid))
        {
            id = TSelf.Create(guid);
            return true;
        }

        id = null;
        return false;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(StronglyTypedIdCrtp<TSelf> id) => id.Value;

    public override string ToString() => Value.ToString();
}