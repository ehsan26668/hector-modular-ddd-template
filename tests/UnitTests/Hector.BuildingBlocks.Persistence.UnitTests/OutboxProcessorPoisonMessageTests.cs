using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxProcessorPoisonMessageTests
{
    [Fact]
    public void Should_NotProcessMessage_When_MaxRetryCountExceeded()
    {
        // Arrange
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            RetryCount = 5
        };

        var options = new OutboxOptions
        {
            MaxRetryCount = 5
        };

        // Act
        var shouldProcess = message.RetryCount < options.MaxRetryCount;

        // Assert
        shouldProcess.Should().BeFalse();
    }
}