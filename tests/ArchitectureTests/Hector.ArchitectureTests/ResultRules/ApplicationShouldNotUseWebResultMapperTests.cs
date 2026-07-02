using FluentAssertions;
using Hector.BuildingBlocks.Application;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class ApplicationShouldNotUseWebResultMapperTests
{
    [Fact]
    public void Should_NotDependOnWebLayer_When_InApplicationAssembly()
    {
        // Arrange
        var applicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(applicationAssembly)
            .Should()
            .NotHaveDependencyOn("Hector.BuildingBlocks.Web")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
