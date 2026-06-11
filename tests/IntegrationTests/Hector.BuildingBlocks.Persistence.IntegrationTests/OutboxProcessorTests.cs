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

    [Fact]
    public async Task Should_PublishMessage_AndMarkAsProcessed_When_MessageIsReady()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(
            mediator,
            NullLogger<OutboxPublisher>.Instance,
            OutboxSerializer);

        var processor = CreateProcessor(context, publisher);

        var domainEvent = new TestDomainEvent(Guid.NewGuid());

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent),
            OccurredOn = DateTime.UtcNow
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.Received(1).PublishAsync(
            Arg.Is<INotification>(e => e is TestDomainEvent),
            Arg.Any<CancellationToken>());

        var processed = await context.OutboxMessages.SingleAsync();

        processed.ProcessedOn.Should().NotBeNull();
        processed.RetryCount.Should().Be(0);
        processed.Error.Should().BeNull();
    }

    [Fact]
    public async Task Should_SkipMessage_When_LockedByAnotherProcessor()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(
            mediator,
            NullLogger<OutboxPublisher>.Instance,
            OutboxSerializer);

        var processor = CreateProcessor(context, publisher);

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(new TestDomainEvent(Guid.NewGuid())),
            OccurredOn = DateTime.UtcNow,
            LockId = Guid.NewGuid(),
            LockedUntil = DateTime.UtcNow.AddMinutes(5)
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.DidNotReceive().PublishAsync(
            Arg.Any<INotification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ProcessMessage_When_LockExpired()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(
            mediator,
            NullLogger<OutboxPublisher>.Instance,
            OutboxSerializer);

        var processor = CreateProcessor(context, publisher);

        var domainEvent = new TestDomainEvent(Guid.NewGuid());

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent),
            OccurredOn = DateTime.UtcNow,
            LockId = Guid.NewGuid(),
            LockedUntil = DateTime.UtcNow.AddMinutes(-1)
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.Received(1).PublishAsync(
            Arg.Any<INotification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_IncrementRetryCount_When_PublishFails()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        mediator
            .PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new Exception("boom"));

        var publisher = new OutboxPublisher(
            mediator,
            NullLogger<OutboxPublisher>.Instance,
            OutboxSerializer);

        var processor = CreateProcessor(context, publisher);

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(new TestDomainEvent(Guid.NewGuid())),
            OccurredOn = DateTime.UtcNow
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        var stored = await context.OutboxMessages.SingleAsync();

        stored.RetryCount.Should().Be(1);
        stored.Error.Should().NotBeNull();
        stored.LockId.Should().BeNull();
    }

    [Fact]
    public async Task Should_NotProcessMessage_When_MaxRetryReached()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(
            mediator,
            NullLogger<OutboxPublisher>.Instance,
            OutboxSerializer);

        var processor = CreateProcessor(context, publisher);

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(new TestDomainEvent(Guid.NewGuid())),
            OccurredOn = DateTime.UtcNow,
            RetryCount = 5
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        await mediator.DidNotReceive().PublishAsync(
            Arg.Any<INotification>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ProcessMessages_InOrder_When_MultipleMessagesExist()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(
            mediator,
            NullLogger<OutboxPublisher>.Instance,
            OutboxSerializer);

        var processor = CreateProcessor(context, publisher);

        var processedEvents = new List<Guid>();
        mediator.PublishAsync(Arg.Any<INotification>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask)
            .AndDoes(x =>
            {
                var domainEvent = (TestDomainEvent)x.ArgAt<INotification>(0);
                processedEvents.Add(domainEvent.AggregateId);
            });

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var id3 = Guid.NewGuid();

        var now = DateTime.UtcNow;
        context.OutboxMessages.AddRange(
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(new TestDomainEvent(id3)),
                OccurredOn = now.AddMinutes(3)
            },
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(new TestDomainEvent(id1)),
                OccurredOn = now.AddMinutes(1)
            },
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = typeof(TestDomainEvent).AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(new TestDomainEvent(id2)),
                OccurredOn = now.AddMinutes(2)
            }
        );
        await context.SaveChangesAsync();

        // Act
        await processor.ProcessAsync(CancellationToken.None);

        // Assert
        processedEvents.Should().HaveCount(3);
        processedEvents.Should().ContainInOrder(id1, id2, id3);
    }
}
