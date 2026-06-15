using System.Text.Json;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxEventSerializationTests
{
    private const string EventName = "test.serialization-domain-event";
    private const int EventVersion = 1;

    private static readonly IOutboxEventSerializer Serializer =
        new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestDomainEvent).Assembly]));

    [Fact]
    public void Should_ReturnAssemblyQualifiedName_When_GetTypeNameIsCalled()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();

        // Act
        var typeName = Serializer.GetTypeName(domainEvent);

        // Assert
        typeName.Should().Be(EventName);
    }

    [Fact]
    public void Should_SerializeEvent_When_EventIsValid()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();

        // Act
        var json = Serializer.Serialize(domainEvent);

        // Assert
        json.Should().NotBeNullOrWhiteSpace();

        var deserialized = JsonSerializer.Deserialize<TestDomainEvent>(json);

        deserialized.Should().NotBeNull();
    }

    [Fact]
    public void Should_DeserializeEvent_When_MessageIsValid()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(domainEvent),
            OccurredOn = DateTime.UtcNow
        };

        // Act
        var result = Serializer.Deserialize(message);

        // Assert
        result.Should().BeOfType<TestDomainEvent>();
    }

    [Fact]
    public void Should_ThrowException_When_EventTypeCannotBeResolved()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "unknown.type",
            Version = EventVersion,
            Content = "{}",
            OccurredOn = DateTime.UtcNow
        };

        // Act
        var action = () => Serializer.Deserialize(message);

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*could not be resolved*");
    }

    [OutboxEvent(EventName, 1)]
    internal sealed record TestDomainEvent : DomainEventBase;
}