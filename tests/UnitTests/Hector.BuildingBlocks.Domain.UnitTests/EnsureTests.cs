using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class EnsureTests
{
    [Fact]
    public void Should_ThrowBusinessRuleViolationException_When_NotNullValueIsNull()
    {
        // Arrange
        object? value = null;

        // Act
        var act = () => Ensure.NotNull(value, "Value cannot be null.");

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Value cannot be null.");
    }

    [Fact]
    public void Should_NotThrowAndReturnValue_When_NotNullValueIsNotNull()
    {
        // Arrange
        var value = new object();

        // Act
        var result = Ensure.NotNull(value, "Value cannot be null.");

        // Assert
        result.Should().BeSameAs(value);
    }

    [Fact]
    public void Should_ThrowBusinessRuleViolationExceptionWithGeneratedMessage_When_NotNullMessageIsNotProvided()
    {
        // Arrange
        object? value = null;

        // Act
        var act = () => Ensure.NotNull(value);

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("'value' must not be null.");
    }

    [Fact]
    public void Should_ThrowBusinessRuleViolationException_When_NotEmptyValueIsNull()
    {
        // Arrange
        string? value = null;

        // Act
        var act = () => Ensure.NotEmpty(value, "Value cannot be empty.");

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Value cannot be empty.");
    }

    [Fact]
    public void Should_ThrowBusinessRuleViolationException_When_NotEmptyValueIsEmpty()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var act = () => Ensure.NotEmpty(value, "Value cannot be empty.");

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Value cannot be empty.");
    }

    [Fact]
    public void Should_ThrowBusinessRuleViolationException_When_NotEmptyValueIsWhitespace()
    {
        // Arrange
        var value = "    ";

        // Act
        var act = () => Ensure.NotEmpty(value, "Value cannot be empty.");

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Value cannot be empty.");
    }

    [Fact]
    public void Should_NotThrowAndReturnValue_When_NotEmptyValueIsNotEmpty()
    {
        // Arrange
        var value = "Project name";

        // Act
        var result = Ensure.NotEmpty(value, "Value cannot be empty.");

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Should_ThrowBusinessRuleViolationExceptionWithGeneratedMessage_When_NotEmptyMessageIsNotProvided()
    {
        // Arrange
        var value = string.Empty;

        // Act
        var act = () => Ensure.NotEmpty(value);

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("'value' must not be empty.");
    }

    [Fact]
    public void Should_ThrowbusinessRuleViolationException_When_NotDefaultGuidValueIsDefault()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var act = () => Ensure.NotDefault(value, "Value cannot be default.");

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Value cannot be default.");
    }

    [Fact]
    public void Should_NotThrowAndReturnValue_When_NotDefaultGuidValueIsNotDefault()
    {
        // Arrange
        var value = Guid.NewGuid();

        // Act
        var result = Ensure.NotDefault(value, "Value cannot be default.");

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Should_ThrowBusinessRuleViolationException_When_NotDefaultIntegerValueIsDefault()
    {
        // Arrange
        var value = default(int);

        // Act
        var act = () => Ensure.NotDefault(value, "Value cannot be default.");

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Value cannot be default.");
    }

    [Fact]
    public void Should_NotThrowAndReturnValue_When_NotDefaultIntegerValueIsNotDefault()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Ensure.NotDefault(value, "Value cannot be default.");

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void Should_ThrowBusinessRuleViolationExceptionWithGeneratedMessage_When_NotDefaultMessageIsNotProvided()
    {
        // Arrange
        var value = Guid.Empty;

        // Act
        var act = () => Ensure.NotDefault(value);

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("'value' must not be default.");
    }

    [Fact]
    public void Should_ThrowBusinessRuleViolationException_When_TrueConditionIsFalse()
    {
        // Arrange
        var condition = false;

        // Act
        var act = () => Ensure.True(condition, "Condition must be true.");

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Condition must be true.");
    }

    [Fact]
    public void Should_NotThrow_When_TrueConditionIsTrue()
    {
        // Arrange
        var condition = true;

        // Act
        var act = () => Ensure.True(condition, "Condition must be true.");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Should_ThrowBusinessRuleViolationExceptionWithGeneratedMessage_When_TrueMessageIsNotProvided()
    {
        // Arrange
        var condition = false;

        // Act
        var act = () => Ensure.True(condition);

        // Assert
        act.Should()
            .Throw<BusinessRuleViolationException>()
            .WithMessage("Condition 'condition' must be true.");
    }
}