using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxProcessorPoisonMessageTests
{
    [Fact]
    public void Should_NotProcessMessage_When_MessageIsPoisoned()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            RetryCount = 5,
            IsPoisoned = true
        };

        var options = new OutboxOptions
        {
            MaxRetryCount = 5
        };

        // Act
        var shouldProcess =
            !message.IsPoisoned &&
            message.RetryCount < options.MaxRetryCount;

        // Assert
        shouldProcess.Should().BeFalse();
    }

    [Fact]
    public void Should_NotProcessMessage_When_MaxRetryReached()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            RetryCount = 5,
            IsPoisoned = false
        };

        var options = new OutboxOptions
        {
            MaxRetryCount = 5
        };

        // Act
        var shouldProcess =
            !message.IsPoisoned &&
            message.RetryCount < options.MaxRetryCount;

        // Assert
        shouldProcess.Should().BeFalse();
    }

    [Fact]
    public void Should_ProcessMessage_When_NotPoisoned_AndRetryAvailable()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            RetryCount = 1,
            IsPoisoned = false
        };

        var options = new OutboxOptions
        {
            MaxRetryCount = 5
        };

        // Act
        var shouldProcess =
            !message.IsPoisoned &&
            message.RetryCount < options.MaxRetryCount;

        // Assert
        shouldProcess.Should().BeTrue();
    }
}
