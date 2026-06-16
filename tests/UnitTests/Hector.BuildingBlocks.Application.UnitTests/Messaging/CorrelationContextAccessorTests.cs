using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging.Correlation;

namespace Hector.BuildingBlocks.Application.UnitTests.Messaging;

public class CorrelationContextAccessorTests
{
    [Fact]
    public void Should_ReturnNewCorrelationId_When_ContextNotSet()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();

        // Act
        var context = accessor.Current;

        // Assert
        context.Should().BeNull();
    }

    [Fact]
    public void Should_ReturnSameContext_When_Set()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        var correlation = new CorrelationContext(Guid.NewGuid(), null, null);

        // Act
        accessor.Set(correlation);
        var result = accessor.Current;

        // Assert
        result.Should().Be(correlation);
    }

    [Fact]
    public void Should_ReturnNull_When_ClearIsCalled()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        var correlation = new CorrelationContext(Guid.NewGuid(), null, null);
        accessor.Set(correlation);

        // Act
        accessor.Clear();
        var result = accessor.Current;

        // Assert
        result.Should().BeNull();
    }
}