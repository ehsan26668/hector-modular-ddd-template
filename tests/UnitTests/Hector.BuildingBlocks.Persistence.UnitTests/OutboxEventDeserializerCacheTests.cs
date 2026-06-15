using System.Text.Json;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxEventDeserializerCacheTests
{
    private const string EventName = "test.deserializer-cache-domain-event";
    private const int EventVersion = 1;

    [Fact]
    public void Should_DeserializeMultipleMessages_When_SameEventTypeIsUsed()
    {
        // Arrange
        var serializer = new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestDomainEvent).Assembly]));

        var domainEvent = new TestDomainEvent();

        var message1 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(domainEvent),
            OccurredOn = DateTime.UtcNow
        };

        var message2 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(domainEvent),
            OccurredOn = DateTime.UtcNow
        };

        // Act
        var result1 = serializer.Deserialize(message1);
        var result2 = serializer.Deserialize(message2);

        // Assert
        result1.Should().BeOfType<TestDomainEvent>();
        result2.Should().BeOfType<TestDomainEvent>();
    }

    [OutboxEvent(EventName, 1)]
    internal sealed record TestDomainEvent : DomainEventBase;
}