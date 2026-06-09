using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class DomainExceptionTests
{
    [Fact]
    public void Should_InheritFromException_When_DomainExceptionTypeIsUsed()
    {
        // Arrange & Act
        var exception = new DomainException("Test error");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    [Fact]
    public void Should_SetMessage_When_DomainExceptionIsCreatedWithMessage()
    {
        // Arrange & Act
        var exception = new DomainException("Test error");

        // Assert
        exception.Message.Should().Be("Test error");
    }

    [Fact]
    public void Should_SetMessageAndInnerException_When_DomainExceptionIsCreatedWithInnerException()
    {
        // Arrange
        var innerException = new Exception("Inner error");

        // Act
        var exception = new DomainException("Outer error", innerException);

        // Assert
        exception.Message.Should().Be("Outer error");
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void Should_InheritFromDomainException_When_BusinessRuleViolationExceptionIsUsed()
    {
        // Arrange & Act
        var exception = new BusinessRuleViolationException("Role broken");

        // Assert
        exception.Should().BeAssignableTo<DomainException>();
    }

    [Fact]
    public void Should_SetMessage_When_BusinessRuleViolationExceptionIsCreatedWithMessage()
    {
        // Arrange & Act
        var exception = new BusinessRuleViolationException("Rule broken");

        // Assert
        exception.Message.Should().Be("Rule broken");
    }

    [Fact]
    public void Should_SetMessageAndInnerException_When_BusinessRuleViolationExceptionIsCreatedWithInnerException()
    {
        // Arrange
        var innerException = new Exception("Inner error");

        // Act
        var exception = new BusinessRuleViolationException("Rule broken", innerException);

        // Assert
        exception.Message.Should().Be("Rule broken");
        exception.InnerException.Should().Be(innerException);
    }
}
