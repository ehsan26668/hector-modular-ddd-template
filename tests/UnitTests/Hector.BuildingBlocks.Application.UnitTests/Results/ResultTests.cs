using FluentAssertions;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.UnitTests.Results;

public sealed class ResultTests
{
    [Fact]
    public void Should_ReturnSuccessfulResult_When_SuccessIsCreated()
    {
        // Arrange

        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void Should_ReturnFailedResult_When_FailureIsCreated()
    {
        // Arrange
        var error = new Error(
            "Test.Error",
            "A test error occurred.",
            ErrorCategory.Unexpected);

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_ErrorIsAccessedFromSuccessfulResult()
    {
        // Arrange
        var result = Result.Success();

        // Act
        Action act = () => _ = result.Error;

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Success result has no error.");
    }

    [Fact]
    public void Should_ReturnSuccessfulGenericResult_When_SuccessIsCreatedWithValue()
    {
        // Arrange
        const string value = "test-value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Should_ReturnFailedGenericResult_When_FailureIsCreatedWithError()
    {
        // Arrange
        var error = new Error(
            "Validation.Failed",
            "Validation failed.",
            ErrorCategory.Validation);

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_ValueIsAccessedFromFailedGenericResult()
    {
        // Arrange
        var error = new Error(
            "Test.Error",
            "A test error occurred.",
            ErrorCategory.Unexpected);

        var result = Result<string>.Failure(error);

        // Act
        Action act = () => _ = result.Value;

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Failure result has no value.");
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_ErrorIsAccessedFromSuccessfulGenericResult()
    {
        // Arrange
        var result = Result<string>.Success("test-value");

        // Act
        Action act = () => _ = result.Error;

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Success result has no error.");
    }
}
