using System.Text.Json;
using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxEventVersioningTests
{
    private const string EventName = "test.versioned-event";

    [Fact]
    public void Should_ResolveDifferentEventTypes_When_VersionsDiffer()
    {
        // Arrange
        var serializer = new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestEventV1).Assembly]));

        var messageV1 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = 1,
            Content = JsonSerializer.Serialize(new TestEventV1()),
            OccurredOn = DateTime.UtcNow
        };

        var messageV2 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = 2,
            Content = JsonSerializer.Serialize(new TestEventV2()),
            OccurredOn = DateTime.UtcNow
        };

        // Act
        var resultV1 = serializer.Deserialize(messageV1);
        var resultV2 = serializer.Deserialize(messageV2);

        // Assert
        resultV1.Should().BeOfType<TestEventV1>();
        resultV2.Should().BeOfType<TestEventV2>();
    }

    [OutboxEvent(EventName, 1)]
    internal sealed record TestEventV1 : DomainEventBase;

    [OutboxEvent(EventName, 2)]
    internal sealed record TestEventV2 : DomainEventBase;
}
