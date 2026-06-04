namespace Hector.BuildingBlocks.Domain.Primitives;

public abstract class StronglyTypedId<TValue> : ValueObject
    where TValue : notnull
{
    public TValue Value { get; }

    protected StronglyTypedId(TValue value)
    {
        Ensure.NotDefault(value, "Strongly typed id value cannot be default.");
        Value = value;
    }

    protected StronglyTypedId(TValue value, bool isEmpty)
    {
        Value = value;
    }

    public static implicit operator TValue(StronglyTypedId<TValue> id) => id.Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value.ToString() ?? string.Empty;
    }
}

public abstract class StronglyTypedId : StronglyTypedId<Guid>
{
    protected StronglyTypedId(Guid value) : base(value)
    {
    }

    protected StronglyTypedId(Guid value, bool isEmpty) : base(value, isEmpty)
    {
    }
}