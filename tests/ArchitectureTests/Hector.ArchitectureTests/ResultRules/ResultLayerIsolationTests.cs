using FluentAssertions;
using Hector.BuildingBlocks.Domain;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class ResultLayerIsolationTests
{
    [Fact]
    public void Should_NotReference_ResultTypes_When_DomainLayerIsAnalyzed()
    {
        // Arrange
        var domainAssembly = typeof(DomainAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(domainAssembly)
            .Should()
            .NotHaveDependencyOn("Hector.BuildingBlocks.Application.Results")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Should_NotReference_ApplicationLayer_When_DomainLayerIsAnalyzed()
    {
        // Arrange
        var domainAssembly = typeof(DomainAssemblyMarker).Assembly;

        // Act
        var result = Types
            .InAssembly(domainAssembly)
            .Should()
            .NotHaveDependencyOn("Hector.BuildingBlocks.Application")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}