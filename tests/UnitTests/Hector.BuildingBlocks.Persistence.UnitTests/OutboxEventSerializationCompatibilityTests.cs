using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxEventSerializationCompatibilityTests
{
    private const string EventName = "test.compatibility-event";
    private const int EventVersion = 1;

    private static readonly IOutboxEventSerializer Serializer =
        new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestEvent).Assembly]));

    [Fact]
    public void Should_IgnoreUnknownJsonProperties_When_DeserializingEvent()
    {
        // Arrange
        var json = """
        {
            "id": "00000000-0000-0000-0000-000000000001",
            "unknownField": "unexpected"
        }
        """;

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = json,
            OccurredOn = DateTime.UtcNow
        };

        // Act
        var result = Serializer.Deserialize(message);

        // Assert
        result.Should().BeOfType<TestEvent>();
    }

    [Fact]
    public void Should_UseDefaultValue_When_NewFieldIsMissing()
    {
        // Arrange
        var v2EventName = EventName;
        var v2Version = EventVersion + 1;

        var json = """
        {
            "projectId": "00000000-0000-0000-0000-000000000001"
        }
        """;

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = v2EventName,
            Version = v2Version,
            Content = json,
            OccurredOn = DateTime.UtcNow
        };

        // Act
        var result = (TestEvent2)Serializer.Deserialize(message);

        // Assert
        result.ProjectId.Should().NotBeEmpty();
        result.CreatedBy.Should().BeNull();
    }

    [OutboxEvent(EventName, EventVersion)]
    internal sealed record TestEvent(Guid Id) : DomainEventBase;

    [OutboxEvent(EventName, EventVersion + 1)]
    internal sealed record TestEvent2(
        Guid ProjectId,
        string? CreatedBy = null
    ) : DomainEventBase;
}