using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using static Hector.Testing.Persistence.PersistenceTestInfrastructure;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxIntegrationEventBusTests
{
    [Fact]
    public async Task Should_ThrowArgumentNullException_When_IntegrationEventIsNull()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var messageFactory = Substitute.For<IOutboxMessageFactory>();
        var bus = new OutboxIntegrationEventBus(context, messageFactory);

        // Act
        var act = async () => await bus.PublishAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Should_AddOutboxMessageToChangeTracker_When_EventIsPublished()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var messageFactory = Substitute.For<IOutboxMessageFactory>();
        var bus = new OutboxIntegrationEventBus(context, messageFactory);

        var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());
        var outboxMessage = new OutboxMessage
        {
            Id = integrationEvent.MessageId,
            Type = typeof(TestIntegrationEvent).FullName!,
            Version = 1,
            Content = "{}",
            OccurredOn = DateTime.UtcNow
        };

        messageFactory.Create(integrationEvent).Returns(outboxMessage);

        // Act
        await bus.PublishAsync(integrationEvent);

        // Assert
        context.ChangeTracker
            .Entries<OutboxMessage>()
            .Should()
            .ContainSingle(e => e.Entity.Id == integrationEvent.MessageId);
    }

    [Fact]
    public async Task Should_PersistOutboxMessage_When_ContextIsSaved()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var messageFactory = Substitute.For<IOutboxMessageFactory>();
        var bus = new OutboxIntegrationEventBus(context, messageFactory);

        var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());
        const string content = "{\"test\":\"value\"}";

        messageFactory.Create(integrationEvent).Returns(new OutboxMessage
        {
            Id = integrationEvent.MessageId,
            Type = typeof(TestIntegrationEvent).FullName!,
            Version = 1,
            Content = content,
            OccurredOn = DateTime.UtcNow
        });

        // Act
        await bus.PublishAsync(integrationEvent);
        await context.SaveChangesAsync();

        // Assert
        var message = await context.OutboxMessages
            .SingleAsync(x => x.Id == integrationEvent.MessageId);

        message.Content.Should().Be(content);
        message.Type.Should().Be(typeof(TestIntegrationEvent).FullName);
        message.ProcessedOn.Should().BeNull();
    }

    [Fact]
    public async Task Should_CreateOutboxMessageUsingFactory_When_EventIsPublished()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var messageFactory = Substitute.For<IOutboxMessageFactory>();
        var bus = new OutboxIntegrationEventBus(context, messageFactory);

        var integrationEvent = new TestIntegrationEvent(Guid.NewGuid());

        messageFactory.Create(integrationEvent).Returns(new OutboxMessage
        {
            Id = integrationEvent.MessageId,
            Type = typeof(TestIntegrationEvent).FullName!,
            Version = 1,
            Content = "{}",
            OccurredOn = DateTime.UtcNow
        });

        // Act
        await bus.PublishAsync(integrationEvent);

        // Assert
        messageFactory.Received(1).Create(integrationEvent);
    }

    private sealed record TestIntegrationEvent(Guid MessageId) : IIntegrationEvent;
}
