using FluentAssertions;
using Hector.BuildingBlocks.Application.Results;

namespace Hector.BuildingBlocks.Application.UnitTests.Results;

public sealed class ValidationErrorTests
{
    [Fact]
    public void Should_ReturnValidationError_When_CreateIsCalled()
    {
        // Arrange
        var failures = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required."]
        };

        // Act
        var error = ValidationError.Create(
            "Validation.Failed",
            "Validation failed.",
            failures);

        // Assert
        error.Code.Should().Be("Validation.Failed");
        error.Message.Should().Be("Validation failed.");
        error.Category.Should().Be(ErrorCategory.Validation);
        error.Metadata.Should().NotBeNull();
        error.Metadata.Should().ContainKey("failures");
        error.Metadata!["failures"].Should().BeSameAs(failures);
    }
}
