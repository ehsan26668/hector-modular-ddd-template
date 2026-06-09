using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxEventSerializationTests
{
    [Fact]
    public void Should_return_assembly_qualified_type_name_when_event_is_serialized()
    {
        // Arrange
        var serializer = new SystemTextJsonOutboxEventSerializer(
            new CachedOutboxEventTypeResolver());

        var domainEvent = new TestDomainEvent();

        // Act
        var typeName = serializer.GetTypeName(domainEvent);

        // Assert
        typeName.Should().Be(typeof(TestDomainEvent).AssemblyQualifiedName);
    }

    [Fact]
    public void Should_deserialize_event_when_valid_outbox_message_is_provided()
    {
        // Arrange
        var serializer = new SystemTextJsonOutboxEventSerializer(
            new CachedOutboxEventTypeResolver());

        var domainEvent = new TestDomainEvent();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = serializer.GetTypeName(domainEvent),
            Content = serializer.Serialize(domainEvent),
            OccurredOn = DateTime.UtcNow
        };

        // Act
        var result = serializer.Deserialize(message);

        // Assert
        result.Should().BeOfType<TestDomainEvent>();
    }

    [Fact]
    public void Should_throw_exception_when_event_type_cannot_be_resolved()
    {
        // Arrange
        var serializer = new SystemTextJsonOutboxEventSerializer(
            new CachedOutboxEventTypeResolver());

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Unknown.Type, Unknown.Assembly",
            Content = "{}",
            OccurredOn = DateTime.UtcNow
        };

        // Act
        var act = () => serializer.Deserialize(message);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    private sealed record TestDomainEvent : DomainEventBase;
}