namespace Hector.BuildingBlocks.Application.Messaging;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class OutboxEventAttribute : Attribute
{
    public OutboxEventAttribute(string name, int version = 1)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Event name cannot be null or whitespace.",
                nameof(name));
        }

        if (version <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(version),
                "Event version must be greater than zero.");
        }

        Name = name;
        Version = version;
    }

    public string Name { get; }
    public int Version { get; }
}