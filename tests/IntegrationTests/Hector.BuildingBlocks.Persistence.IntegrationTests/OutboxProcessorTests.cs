using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using static Hector.Persistence.Testing.PersistenceTestInfrastructure;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class OutboxProcessorTests
{
    private const string EventName = "test.persistence-domain-event";
    private const int EventVersion = 1;

    private static OutboxProcessor CreateProcessor(
        HectorDbContext context,
        IOutboxPublisher publisher,
        OutboxOptions? options = null)
    {
        return new OutboxProcessor(
            context,
            publisher,
            NullLogger<OutboxProcessor>.Instance,
            Options.Create(options ?? new OutboxOptions()));
    }

    // ------------------------------
    // 1️⃣ Happy path
    // ------------------------------

    [Fact]
    public async Task Should_PublishMessage_AndMarkAsProcessed_When_MessageIsReady()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var serializer = Substitute.For<IOutboxEventSerializer>();
        var publisher = new OutboxPublisher(mediator, serializer);
        var processor = CreateProcessor(context, publisher);

        var integrationEvent = Substitute.For<INotification>();

        serializer.Deserialize(Arg.Any<OutboxMessage>())
            .Returns(integrationEvent);

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = "{}",
            OccurredOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.Received(1)
            .PublishAsync(integrationEvent, Arg.Any<CancellationToken>());

        var stored = await context.OutboxMessages.SingleAsync();
        stored.ProcessedOn.Should().NotBeNull();
        stored.IsPoisoned.Should().BeFalse();
        stored.RetryCount.Should().Be(0);
        stored.Error.Should().BeNull();
    }


    // ------------------------------
    // 3️⃣ Retry & Failure
    // ------------------------------

    [Fact]
    public async Task Should_IncrementRetryCount_AndPersistError_When_PublishFails()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var serializer = Substitute.For<IOutboxEventSerializer>();
        var integrationEvent = Substitute.For<INotification>();

        serializer.Deserialize(Arg.Any<OutboxMessage>())
            .Returns(integrationEvent);

        mediator.PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new Exception("boom"));

        var publisher = new OutboxPublisher(mediator, serializer);
        var processor = CreateProcessor(context, publisher);

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = "{}",
            OccurredOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        var stored = await context.OutboxMessages.SingleAsync();
        stored.RetryCount.Should().Be(1);
        stored.Error.Should().Contain("boom");
        stored.IsPoisoned.Should().BeFalse();
        stored.ProcessedOn.Should().BeNull();
    }

    [Fact]
    public async Task Should_MarkAsPoisoned_When_MaxRetryReached()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var serializer = Substitute.For<IOutboxEventSerializer>();
        var integrationEvent = Substitute.For<INotification>();

        serializer.Deserialize(Arg.Any<OutboxMessage>())
            .Returns(integrationEvent);

        mediator.PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new Exception("permanent failure"));

        var publisher = new OutboxPublisher(mediator, serializer);
        var options = new OutboxOptions { MaxRetryCount = 1 };
        var processor = CreateProcessor(context, publisher, options);

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = "{}",
            OccurredOn = DateTime.UtcNow,
            RetryCount = 0
        });

        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        var stored = await context.OutboxMessages.SingleAsync();
        stored.IsPoisoned.Should().BeTrue();
        stored.FailedOn.Should().NotBeNull();
        stored.FailureReason.Should().Contain("permanent failure");
        stored.RetryCount.Should().Be(1);

        await mediator.Received(1)
            .PublishAsync(integrationEvent, Arg.Any<CancellationToken>());
    }

    // ------------------------------
    // 5️⃣ Edge cases
    // ------------------------------

    [Fact]
    public async Task Should_IncrementRetry_When_TypeCannotBeResolved()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var serializer = Substitute.For<IOutboxEventSerializer>();

        serializer.Deserialize(Arg.Any<OutboxMessage>())
            .Throws(new Exception("Deserialization failed"));

        var publisher = new OutboxPublisher(mediator, serializer);
        var processor = CreateProcessor(context, publisher);

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "Missing.Type",
            Version = EventVersion,
            Content = "{}",
            OccurredOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        var stored = await context.OutboxMessages.SingleAsync();
        stored.RetryCount.Should().Be(1);
        stored.IsPoisoned.Should().BeFalse();
        stored.ProcessedOn.Should().BeNull();

        await mediator.DidNotReceive()
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }
}
