using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Hector.Testing.Persistence.PersistenceTestInfrastructure;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class OutboxCleanupTests
{
    [Fact]
    public async Task Should_DeleteProcessedMessages_When_RetentionPeriodExpired()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
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

        var cleaner = CreateCleaner(
            context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 100);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        var count = await context.OutboxMessages.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Should_KeepRecentMessages_When_RetentionPeriodNotExpired()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
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

        var cleaner = CreateCleaner(
            context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 100);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        var count = await context.OutboxMessages.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Should_NotDeleteUnprocessedMessages_When_MessageIsOlderThanRetentionPeriod()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
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

        var cleaner = CreateCleaner(
            context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 100);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        var count = await context.OutboxMessages.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task Should_DeleteOnlyBatchSizeMessages_When_EligibleMessagesExceedBatchLimit()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        for (var index = 0; index < 10; index++)
        {
            context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-30),
                ProcessedOn = DateTime.UtcNow.AddDays(-20).AddMinutes(index)
            });
        }

        await context.SaveChangesAsync();

        var cleaner = CreateCleaner(
            context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 5);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        var remaining = await context.OutboxMessages.CountAsync();
        remaining.Should().Be(5);
    }

    [Fact]
    public async Task Should_DoNothing_When_NoEligibleMessagesExist()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        context.OutboxMessages.AddRange(
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-2),
                ProcessedOn = DateTime.UtcNow.AddDays(-1)
            },
            new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-30),
                ProcessedOn = null
            });

        await context.SaveChangesAsync();

        var cleaner = CreateCleaner(
            context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 100);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        var remaining = await context.OutboxMessages.CountAsync();
        remaining.Should().Be(2);
    }

    [Fact]
    public async Task Should_DeleteOldestProcessedMessagesFirst_When_BatchSizeIsLimited()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var oldestId = Guid.NewGuid();
        var middleId = Guid.NewGuid();
        var newestId = Guid.NewGuid();

        context.OutboxMessages.AddRange(
            new OutboxMessage
            {
                Id = oldestId,
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-30),
                ProcessedOn = DateTime.UtcNow.AddDays(-20)
            },
            new OutboxMessage
            {
                Id = middleId,
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-29),
                ProcessedOn = DateTime.UtcNow.AddDays(-19)
            },
            new OutboxMessage
            {
                Id = newestId,
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-28),
                ProcessedOn = DateTime.UtcNow.AddDays(-18)
            });

        await context.SaveChangesAsync();

        var cleaner = CreateCleaner(
            context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 2);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        var remainingMessages = await context.OutboxMessages
            .OrderBy(message => message.ProcessedOn)
            .ToListAsync();

        remainingMessages.Should().HaveCount(1);
        remainingMessages[0].Id.Should().Be(newestId);
    }

    [Fact]
    public async Task Should_DeleteNextBatch_When_CleanupRunsMultipleTimes()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        for (var index = 0; index < 6; index++)
        {
            context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "test",
                Content = "{}",
                OccurredOn = DateTime.UtcNow.AddDays(-30),
                ProcessedOn = DateTime.UtcNow.AddDays(-20).AddMinutes(index)
            });
        }

        await context.SaveChangesAsync();

        var cleaner = CreateCleaner(
            context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 2);

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        var remaining = await context.OutboxMessages.CountAsync();
        remaining.Should().Be(2);
    }

    [Fact]
    public async Task Should_Throw_When_RetentionPeriodIsNotPositive()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var cleaner = CreateCleaner(
            context,
            retentionPeriod: TimeSpan.Zero,
            cleanupBatchSize: 100);

        // Act
        var action = async () => await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*retention period*");
    }

    [Fact]
    public async Task Should_Throw_When_CleanupBatchSizeIsNotPositive()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var cleaner = CreateCleaner(
            context,
            retentionPeriod: TimeSpan.FromDays(7),
            cleanupBatchSize: 0);

        // Act
        var action = async () => await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*batch size*");
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
