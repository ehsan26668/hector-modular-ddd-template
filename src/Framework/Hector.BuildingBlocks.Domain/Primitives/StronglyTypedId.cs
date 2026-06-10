namespace Hector.BuildingBlocks.Domain.Primitives;

/// <summary>
/// Base class for strongly typed identifiers backed by Guid.
/// Strongly typed identifiers prevent accidental misuse of primitive identifiers
/// across aggregate and entity boundaries.
/// </summary>
public abstract class StronglyTypedId<TSelf> : ValueObject
    where TSelf : StronglyTypedId<TSelf>
{
    protected StronglyTypedId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Strongly typed id value cannot be empty.", nameof(value));

        Value = value;
    }

    public Guid Value { get; }

    /// <summary>
    /// Creates a new strongly typed identifier using Guid version 7.
    /// </summary>
    protected static TSelf CreateNew(Func<Guid, TSelf> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return factory(Guid.CreateVersion7());
    }

    /// <summary>
    /// Creates a strongly typed identifier from an existing Guid value.
    /// Typically used for rehydration from persistence.
    /// </summary>
    protected static TSelf FromExisting(Guid value, Func<Guid, TSelf> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        return factory(value);
    }

    public static implicit operator Guid(StronglyTypedId<TSelf> id)
        => id?.Value ?? throw new ArgumentNullException(nameof(id));

    public override string ToString()
        => Value.ToString();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
