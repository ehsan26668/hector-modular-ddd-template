using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class EnsureTests
{
    [Fact]
    public void NotNull_Should_Throw_Exception_When_Value_Is_Null()
    {
        // Arrange
        object? value = null;

        // Act
        var act = () => Ensure.NotNull(value, "Value cannot be null");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Value cannot be null");
    }

    [Fact]
    public void NotNull_Should_Not_Throw_When_Value_Is_Not_Null()
    {
        // Arrange
        var value = new object();

        // Act
        var act = () => Ensure.NotNull(value, "Value cannot be null");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotEmpty_Should_Throw_When_String_Is_Empty()
    {
        // Arrange
        var value ="";

        // Act
        var act = () => Ensure.NotEmpty(value, "Value cannot be empty");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Value cannot be empty");
    }

    [Fact]
    public void NotEmpty_Should_Throw_When_String_Is_Whitespace()
    {
        // Arrange
        var value = " ";

        // Act
        var act = () => Ensure.NotEmpty(value, "Value cannot be empty");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void True_Should_Throw_When_Condition_Is_False()
    {
        // Act
        var act = () => Ensure.True(false, "Condition must be true");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Condition must be true");
    }

    [Fact]
    public void True_Should_Not_Throw_When_Condition_Is_True()
    {
        // Act
        var act = () => Ensure.True(true, "Condition must be true");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotDefault_Should_Throw_When_Value_Is_Default()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var act = () => Ensure.NotDefault(value, "Value cannot be default");

        // Assert
        act.Should().Throw<BusinessRuleViolationException>()
            .WithMessage("Value cannot be default");
    }

    [Fact]
    public void NotDefault_Should_Not_Throw_When_Value_Is_Not_Default()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var act = () => Ensure.NotDefault(value, "Value cannot be default");

        // Assert
        act.Should().NotThrow();
    }
}