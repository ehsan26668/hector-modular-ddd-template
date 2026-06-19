using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Hector.Persistence.Testing.PersistenceTestInfrastructure;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class OutboxCleanupTests
{
    [Fact]
    public async Task Should_DeleteProcessedMessages_When_RetentionPeriodExpired()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "test",
            Content = "{}",
            OccurredOn = DateTime.UtcNow.AddDays(-10),
            ProcessedOn = DateTime.UtcNow.AddDays(-9),
            IsPoisoned = false
        });

        await context.SaveChangesAsync();

        var cleaner = CreateCleaner(context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 100);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        (await context.OutboxMessages.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Should_DeletePoisonedMessages_When_RetentionPeriodExpired()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "test",
            Content = "{}",
            OccurredOn = DateTime.UtcNow.AddDays(-30),
            FailedOn = DateTime.UtcNow.AddDays(-20),
            IsPoisoned = true
        });

        await context.SaveChangesAsync();

        var cleaner = CreateCleaner(context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 100);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        (await context.OutboxMessages.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Should_NotDeleteUnprocessedMessages_When_OlderThanRetentionPeriod()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        context.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "test",
            Content = "{}",
            OccurredOn = DateTime.UtcNow.AddDays(-30),
            ProcessedOn = null,
            IsPoisoned = false
        });

        await context.SaveChangesAsync();

        var cleaner = CreateCleaner(context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 100);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        (await context.OutboxMessages.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Should_DeleteOldestEligibleMessagesFirst_When_BatchLimited()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var oldest = Guid.NewGuid();
        var middle = Guid.NewGuid();
        var newest = Guid.NewGuid();

        context.OutboxMessages.AddRange(
            new OutboxMessage
            {
                Id = oldest,
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-30),
                ProcessedOn = DateTime.UtcNow.AddDays(-20)
            },
            new OutboxMessage
            {
                Id = middle,
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-29),
                ProcessedOn = DateTime.UtcNow.AddDays(-19)
            },
            new OutboxMessage
            {
                Id = newest,
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-28),
                ProcessedOn = DateTime.UtcNow.AddDays(-18)
            });

        await context.SaveChangesAsync();

        var cleaner = CreateCleaner(context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 2);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        var remaining = await context.OutboxMessages.ToListAsync();

        remaining.Should().HaveCount(1);
        remaining.Single().Id.Should().Be(newest);
    }

    [Fact]
    public async Task Should_Throw_When_RetentionPeriodIsNotPositive()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var cleaner = CreateCleaner(context,
            retentionPeriod: TimeSpan.Zero,
            cleanupBatchSize: 100);

        // Act
        var act = async () => await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*retention*");
    }

    [Fact]
    public async Task Should_Throw_When_BatchSizeIsNotPositive()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var cleaner = CreateCleaner(context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 0);

        // Act
        var act = async () => await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*batch*");
    }

    private static OutboxCleaner CreateCleaner(
        HectorDbContext context,
        TimeSpan retentionPeriod,
        int cleanupBatchSize)
    {
        var options = new OutboxOptions
        {
            RetentionPeriod = retentionPeriod,
            CleanupBatchSize = cleanupBatchSize
        };

        return new OutboxCleaner(context, Options.Create(options));
    }
}
