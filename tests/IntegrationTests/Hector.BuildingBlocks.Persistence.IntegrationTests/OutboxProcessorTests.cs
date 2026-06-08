using System.Text.Json;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class OutboxProcessorTests
{
    private static readonly IStronglyTypedIdAssemblyProvider StronglyTypedIdAssemblyProvider =
        new TestStronglyTypedIdAssemblyProvider();

    private static async Task<TestDbContext> CreateContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestDbContext(options, StronglyTypedIdAssemblyProvider);

        await context.Database.EnsureCreatedAsync();

        return context;
    }

    public sealed record TestDomainEvent(Guid AggregateId) : DomainEventBase;

    [Fact]
    public async Task ProcessAsync_ShouldPublishMessage_AndMarkAsProcessed()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        await using var context = await CreateContextAsync(connection);

        var mediator = Substitute.For<IMediator>();
        var publisher = new OutboxPublisher(
            mediator,
            NullLogger<OutboxPublisher>.Instance,
            new CachedOutboxEventTypeResolver());
        var processor = new OutboxProcessor(context, publisher, NullLogger<OutboxProcessor>.Instance);

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

        await processor.ProcessAsync(CancellationToken.None);

        await mediator.Received(1).PublishAsync(
            Arg.Is<INotification>(e => e is TestDomainEvent),
            Arg.Any<CancellationToken>());

        var processed = await context.OutboxMessages.SingleAsync();

        processed.ProcessedOn.Should().NotBeNull();
        processed.RetryCount.Should().Be(0);
        processed.Error.Should().BeNull();
    }
}
