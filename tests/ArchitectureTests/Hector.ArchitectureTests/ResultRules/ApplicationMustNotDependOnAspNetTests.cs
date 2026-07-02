using FluentAssertions;
using Hector.BuildingBlocks.Application;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public class ApplicationMustNotDependOnAspNetTests
{
    [Fact]
    public void Should_NotDependOnAspNetCore_When_InApplicationAssembly()
    {
        // Arrange
        var applicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(applicationAssembly)
            .Should()
            .NotHaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
