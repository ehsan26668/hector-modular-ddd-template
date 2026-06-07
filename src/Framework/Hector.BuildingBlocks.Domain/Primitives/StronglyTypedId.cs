namespace Hector.BuildingBlocks.Domain.Primitives;

/// <summary>
/// Base class for strongly typed identifiers based on Guid (v7).
/// Fully domain-safe and persistence-friendly.
/// </summary>
public abstract class StronglyTypedId<TSelf> : ValueObject
    where TSelf : StronglyTypedId<TSelf>
{
    public Guid Value { get; }

    protected StronglyTypedId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("StronglyTypedId cannot be empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Used by concrete types to generate new identifiers (Guid v7).
    /// </summary>
    protected static TSelf CreateNew(Func<Guid, TSelf> factory)
        => factory(Guid.CreateVersion7());

    /// <summary>
    /// Used for rehydration from persistence layer.
    /// </summary>
    protected static TSelf FromExisting(Guid value, Func<Guid, TSelf> factory)
        => factory(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator Guid(StronglyTypedId<TSelf> id)
        => id.Value;

    public override string ToString()
        => Value.ToString();
}
