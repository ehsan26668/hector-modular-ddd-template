using System.Text.Json;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxPublisherTests
{
    private const string EventName = "test.publisher-domain-event";
    private const int EventVersion = 1;

    [Fact]
    public async Task Should_publish_unprocessed_messages()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(new TestDomainEvent()),
            OccurredOn = DateTime.UtcNow
        };

        var messages = new List<OutboxMessage> { message };

        var serializer = new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestDomainEvent).Assembly]));

        var publisher = new OutboxPublisher(
            mediator,
            serializer);

        // Act
        await publisher.PublishAsync(messages, CancellationToken.None);

        // Assert
        await mediator.Received(1).PublishAsync(
            Arg.Any<INotification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_publish_multiple_messages()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();

        var message1 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(new TestDomainEvent()),
            OccurredOn = DateTime.UtcNow
        };

        var message2 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(new TestDomainEvent()),
            OccurredOn = DateTime.UtcNow
        };

        var messages = new List<OutboxMessage>
        {
            message1,
            message2
        };

        var serializer = new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestDomainEvent).Assembly]));

        var publisher = new OutboxPublisher(
            mediator,
            serializer);

        // Act
        await publisher.PublishAsync(messages, CancellationToken.None);

        // Assert
        await mediator.Received(2).PublishAsync(
            Arg.Any<INotification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_throw_when_deserialization_fails()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = "invalid-json",
            OccurredOn = DateTime.UtcNow
        };

        var messages = new List<OutboxMessage> { message };

        var serializer = new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestDomainEvent).Assembly]));

        var publisher = new OutboxPublisher(
            mediator,
            serializer);

        // Act
        var act = () => publisher.PublishAsync(messages, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<JsonException>(act);

        await mediator.DidNotReceive().PublishAsync(
            Arg.Any<INotification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_publish_messages_in_order()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();

        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        var message1 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(event1),
            OccurredOn = DateTime.UtcNow
        };

        var message2 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(event2),
            OccurredOn = DateTime.UtcNow
        };

        var messages = new List<OutboxMessage>
        {
            message1,
            message2
        };

        var serializer = new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestDomainEvent).Assembly]));

        var publisher = new OutboxPublisher(
            mediator,
            serializer);

        var publishedEvents = new List<INotification>();

        mediator
            .When(x => x.PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>()))
            .Do(call => publishedEvents.Add(call.Arg<INotification>()));

        // Act
        await publisher.PublishAsync(messages, CancellationToken.None);

        // Assert
        Assert.Equal(2, publishedEvents.Count);
        Assert.IsType<TestDomainEvent>(publishedEvents[0]);
        Assert.IsType<TestDomainEvent>(publishedEvents[1]);
    }

    [Fact]
    public async Task Should_stop_publishing_when_a_message_fails()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();

        var message1 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(new TestDomainEvent()),
            OccurredOn = DateTime.UtcNow
        };

        var message2 = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(new TestDomainEvent()),
            OccurredOn = DateTime.UtcNow
        };

        var messages = new List<OutboxMessage> { message1, message2 };

        mediator
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException());

        var serializer = new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestDomainEvent).Assembly]));

        var publisher = new OutboxPublisher(mediator, serializer);

        // Act
        var act = () => publisher.PublishAsync(messages, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(act);

        await mediator.Received(1).PublishAsync(
            Arg.Any<INotification>(),
            Arg.Any<CancellationToken>());
    }

    [OutboxEvent(EventName, EventVersion)]
    internal sealed record TestDomainEvent : DomainEventBase;
}
