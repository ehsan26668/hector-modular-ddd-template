using System.Text.Json;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using static Hector.Testing.Persistence.PersistenceTestInfrastructure;

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
        var publisher = new OutboxPublisher(mediator, OutboxSerializer);
        var processor = CreateProcessor(context, publisher);

        var domainEvent = new TestDomainEvent(Guid.NewGuid());

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(domainEvent),
            OccurredOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.Received(1)
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>());

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
        mediator.PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new Exception("boom"));

        var publisher = new OutboxPublisher(mediator, OutboxSerializer);
        var processor = CreateProcessor(context, publisher);

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(new TestDomainEvent(Guid.NewGuid())),
            OccurredOn = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        var stored = await context.OutboxMessages.SingleAsync();
        stored.RetryCount.Should().Be(1);
        stored.Error.Should().Contain("boom");
        stored.IsPoisoned.Should().BeFalse(); // Still recoverable
        stored.ProcessedOn.Should().BeNull();
    }

    [Fact]
    public async Task Should_MarkAsPoisoned_When_MaxRetryReached()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        mediator.PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new Exception("permanent failure"));

        var publisher = new OutboxPublisher(mediator, OutboxSerializer);
        // تنظیم حداکثر تلاش برای تست سریع‌تر
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

        // اطمینان از اینکه دیگر پردازش نمی‌شود
        await mediator.Received(1).PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
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
        var publisher = new OutboxPublisher(mediator, OutboxSerializer);
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
    }
}
