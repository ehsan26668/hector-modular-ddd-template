using FluentAssertions;
using Hector.BuildingBlocks.Application;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public class ValidationExceptionUsageTests
{
    [Fact]
    public void ApplicationLayer_Should_NotDependOn_FluentValidationException()
    {
        // Arrange
        var applicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(applicationAssembly)
            .Should()
            .NotHaveDependencyOn("FluentValidation.ValidationException")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
