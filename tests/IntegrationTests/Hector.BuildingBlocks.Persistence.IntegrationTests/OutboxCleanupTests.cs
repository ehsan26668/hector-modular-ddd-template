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

        var options = new OutboxOptions
        {
            RetentionPeriod = TimeSpan.FromDays(7),
            CleanupBatchSize = 100
        };

        var cleaner = new OutboxCleaner(context, Options.Create(options));

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

        var options = new OutboxOptions
        {
            RetentionPeriod = TimeSpan.FromDays(7)
        };

        var cleaner = new OutboxCleaner(context, Options.Create(options));

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

        var options = new OutboxOptions
        {
            RetentionPeriod = TimeSpan.FromDays(7)
        };

        var cleaner = new OutboxCleaner(context, Options.Create(options));

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

        // Act
        await cleaner.CleanupAsync(CancellationToken.None);

        // Assert
        var remaining = await context.OutboxMessages.CountAsync();
        remaining.Should().Be(5);
    }
}
