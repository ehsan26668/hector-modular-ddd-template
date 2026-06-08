using System.Text.Json;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxPublisherTests
{
    [Fact]
    public async Task Should_publish_unprocessed_messages()
    {
        // Arrange
        var mediator = Substitute.For<IMediator>();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(new TestDomainEvent()),
            OccurredOn = DateTime.UtcNow
        };

        var messages = new List<OutboxMessage> { message };

        var publisher = new OutboxPublisher(
            mediator,
            NullLogger<OutboxPublisher>.Instance,
            new CachedOutboxEventTypeResolver());

        // Act
        await publisher.PublishAsync(messages);

        // Assert
        await mediator.Received(1).PublishAsync(Arg.Any<INotification>());
    }

    internal sealed record TestDomainEvent : DomainEventBase;
}
