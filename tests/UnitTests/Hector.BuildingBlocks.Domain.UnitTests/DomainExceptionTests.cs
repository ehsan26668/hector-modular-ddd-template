using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class DomainExceptionTests
{
    [Fact]
    public void DomainException_Should_Set_Message()
    {
        // Arrange & Act
        var exception = new DomainException("Test error");

        // Assert
        exception.Message.Should().Be("Test error");
    }

    [Fact]
    public void BusinessRuleViolationException_Should_Be_DomainException()
    {
        // Arrange & Act
        var exception = new BusinessRuleViolationException("Rule broken");

        // Assert
        exception.Should().BeAssignableTo<DomainException>()
            .Which.Message.Should().Be("Rule broken");
    }
}
