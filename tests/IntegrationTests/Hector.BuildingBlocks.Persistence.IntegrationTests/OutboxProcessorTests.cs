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
        stored.RetryCount.Should().Be(0);
        stored.Error.Should().BeNull();
    }

    // ------------------------------
    // 2️⃣ Locking
    // ------------------------------

    [Fact]
    public async Task Should_SkipMessage_When_LockedByAnotherProcessor()
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
            Type = EventName,
            Version = EventVersion,
            Content = "{}",
            OccurredOn = DateTime.UtcNow,
            LockId = Guid.NewGuid(),
            LockedUntil = DateTime.UtcNow.AddMinutes(5)
        });

        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.DidNotReceive()
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ProcessMessage_When_LockExpired()
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
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(new TestDomainEvent(Guid.NewGuid())),
            OccurredOn = DateTime.UtcNow,
            LockId = Guid.NewGuid(),
            LockedUntil = DateTime.UtcNow.AddMinutes(-1)
        });

        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.Received(1)
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
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
        stored.LockId.Should().BeNull();
        stored.LockedUntil.Should().NotBeNull();
        stored.LockedUntil.Should().BeAfter(DateTime.UtcNow);
        stored.ProcessedOn.Should().BeNull();
    }

    [Fact]
    public async Task Should_NotProcessMessage_When_MaxRetryReached()
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
            Type = EventName,
            Version = EventVersion,
            Content = "{}",
            OccurredOn = DateTime.UtcNow,
            RetryCount = 5
        });

        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.DidNotReceive()
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    // ------------------------------
    // 4️⃣ Batch & Ordering
    // ------------------------------

    [Fact]
    public async Task Should_ProcessOnlyBatchSizeMessages()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(mediator, OutboxSerializer);

        var processor = CreateProcessor(context, publisher,
            new OutboxOptions { BatchSize = 2 });

        for (int i = 0; i < 5; i++)
        {
            context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = EventName,
                Version = EventVersion,
                Content = JsonSerializer.Serialize(new TestDomainEvent(Guid.NewGuid())),
                OccurredOn = DateTime.UtcNow.AddMinutes(i)
            });
        }

        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.Received(2)
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ProcessMessages_InOrder()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(mediator, OutboxSerializer);
        var processor = CreateProcessor(context, publisher);

        var processed = new List<Guid>();

        mediator.PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(x =>
            {
                processed.Add(((TestDomainEvent)x.ArgAt<INotification>(0)).AggregateId);
            });

        var now = DateTime.UtcNow;

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        context.OutboxMessages.AddRange(
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = EventName,
                Version = EventVersion,
                Content = JsonSerializer.Serialize(new TestDomainEvent(id3)),
                OccurredOn = now.AddMinutes(3)
            },
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = EventName,
                Version = EventVersion,
                Content = JsonSerializer.Serialize(new TestDomainEvent(id1)),
                OccurredOn = now.AddMinutes(1)
            },
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = EventName,
                Version = EventVersion,
                Content = JsonSerializer.Serialize(new TestDomainEvent(id2)),
                OccurredOn = now.AddMinutes(2)
            });

        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        processed.Should().ContainInOrder(id1, id2, id3);
    }

    // ------------------------------
    // 5️⃣ Edge cases
    // ------------------------------

    [Fact]
    public async Task Should_DoNothing_When_NoEligibleMessages()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(mediator, OutboxSerializer);
        var processor = CreateProcessor(context, publisher);

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.DidNotReceive()
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_IncrementRetry_When_TypeCannotBeResolved()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(mediator, OutboxSerializer);
        var processor = CreateProcessor(context, publisher);

        var id = Guid.NewGuid();

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = id,
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
        stored.Error.Should().NotBeNull();
        stored.ProcessedOn.Should().BeNull();
    }

    // ------------------------------
    // 6️⃣ Concurrency safety
    // ------------------------------

    [Fact]
    public async Task Should_AllowOnlyOneProcessor_ToProcessMessage()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();

        await using var context1 = await CreateContextAsync(connection);
        await using var context2 = await CreateContextAsync(connection);

        var mediator1 = Substitute.For<IMediator>();
        var mediator2 = Substitute.For<IMediator>();

        var processor1 = CreateProcessor(context1,
            new OutboxPublisher(mediator1, OutboxSerializer));

        var processor2 = CreateProcessor(context2,
            new OutboxPublisher(mediator2, OutboxSerializer));

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = EventName,
            Version = EventVersion,
            Content = JsonSerializer.Serialize(new TestDomainEvent(Guid.NewGuid())),
            OccurredOn = DateTime.UtcNow
        };

        context1.OutboxMessages.Add(message);
        await context1.SaveChangesAsync();

        // Act
        await Task.WhenAll(
            processor1.ProcessAsync(CancellationToken.None),
            processor2.ProcessAsync(CancellationToken.None));

        // Assert
        var totalCalls =
            mediator1.ReceivedCalls().Count() +
            mediator2.ReceivedCalls().Count();

        totalCalls.Should().Be(1);
    }
}
