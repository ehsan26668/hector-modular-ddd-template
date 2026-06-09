using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class OutboxCleanupTests
{
    private static readonly IStronglyTypedIdAssemblyProvider StronglyTypedIdAssemblyProvider =
        new TestStronglyTypedIdAssemblyProvider();

    private static IOutboxEventSerializer CreateOutboxSerializer()
        => new SystemTextJsonOutboxEventSerializer(
            new CachedOutboxEventTypeResolver());

    private static async Task<TestDbContext> CreateContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        var outboxSerializer = CreateOutboxSerializer();

        var context = new TestDbContext(options, StronglyTypedIdAssemblyProvider, outboxSerializer);

        await context.Database.EnsureCreatedAsync();

        return context;
    }

    public sealed record TestDomainEvent(Guid AggregateId) : DomainEventBase;

    [Fact]
    public async Task Should_DeleteProcessedMessages_When_RetentionPeriodExpired()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = await CreateContextAsync(connection);

        var oldMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "test",
            Content = "{}",
            OccurredOn = DateTime.UtcNow.AddDays(-10),
            ProcessedOn = DateTime.UtcNow.AddDays(-9)
        };

        context.OutboxMessages.Add(oldMessage);
        await context.SaveChangesAsync();

        var options = new OutboxOptions
        {
            RetentionPeriod = TimeSpan.FromDays(7),
            CleanupBatchSize = 100
        };

        var cleaner = new OutboxCleaner(context, Options.Create(options));

        await cleaner.CleanupAsync(CancellationToken.None);

        var count = await context.OutboxMessages.CountAsync();

        count.Should().Be(0);
    }

    [Fact]
    public async Task Should_KeepRecentMessages_When_RetentionPeriodNotExpired()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = await CreateContextAsync(connection);

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "test",
            Content = "{}",
            OccurredOn = DateTime.UtcNow,
            ProcessedOn = DateTime.UtcNow
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();

        var options = new OutboxOptions
        {
            RetentionPeriod = TimeSpan.FromDays(7)
        };

        var cleaner = new OutboxCleaner(context, Options.Create(options));

        await cleaner.CleanupAsync(CancellationToken.None);

        var count = await context.OutboxMessages.CountAsync();

        count.Should().Be(1);
    }

    [Fact]
    public async Task Should_NotDeleteUnprocessedMessages()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = await CreateContextAsync(connection);

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "test",
            Content = "{}",
            OccurredOn = DateTime.UtcNow.AddDays(-30),
            ProcessedOn = null
        };

        context.OutboxMessages.Add(message);
        await context.SaveChangesAsync();

        var options = new OutboxOptions
        {
            RetentionPeriod = TimeSpan.FromDays(7)
        };

        var cleaner = new OutboxCleaner(context, Options.Create(options));

        await cleaner.CleanupAsync(CancellationToken.None);

        var count = await context.OutboxMessages.CountAsync();

        count.Should().Be(1);
    }

    [Fact]
    public async Task Should_DeleteOnlyBatchSizeMessages()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = await CreateContextAsync(connection);

        for (int i = 0; i < 10; i++)
        {
            context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-30),
                ProcessedOn = DateTime.UtcNow.AddDays(-20)
            });
        }

        await context.SaveChangesAsync();

        var options = new OutboxOptions
        {
            RetentionPeriod = TimeSpan.FromDays(7),
            CleanupBatchSize = 5
        };

        var cleaner = new OutboxCleaner(context, Options.Create(options));

        await cleaner.CleanupAsync(CancellationToken.None);

        var remaining = await context.OutboxMessages.CountAsync();

        remaining.Should().Be(5);
    }
}