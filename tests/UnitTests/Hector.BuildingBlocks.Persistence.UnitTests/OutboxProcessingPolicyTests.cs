using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public class OutboxProcessingPolicyTests
{
    [Fact]
    public void Should_ReturnFalse_When_MessageIsPoisoned()
    {
        // Arrange
        var message = new OutboxMessage
        {
            IsPoisoned = true,
            RetryCount = 0
        };

        var options = new OutboxOptions
        {
            MaxRetryCount = 5
        };

        // Act
        var result = OutboxProcessingPolicy.IsProcessable(message, options);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFalse_When_RetryCountExceeded()
    {
        // Arrange
        var message = new OutboxMessage
        {
            IsPoisoned = false,
            RetryCount = 5
        };

        var options = new OutboxOptions
        {
            MaxRetryCount = 5
        };

        // Act
        var result = OutboxProcessingPolicy.IsProcessable(message, options);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnTrue_When_MessageIsValid()
    {
        // Arrange
        var message = new OutboxMessage
        {
            IsPoisoned = false,
            RetryCount = 1
        };

        var options = new OutboxOptions
        {
            MaxRetryCount = 5
        };

        // Act
        var result = OutboxProcessingPolicy.IsProcessable(message, options);

        // Assert
        result.Should().BeTrue();
    }
}
