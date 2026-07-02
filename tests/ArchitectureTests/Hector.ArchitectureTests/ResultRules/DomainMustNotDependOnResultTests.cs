using FluentAssertions;
using Hector.BuildingBlocks.Domain;
using NetArchTest.Rules;

namespace Hector.ArchitectureTests.ResultRules;

public sealed class DomainMustNotDependOnResultTests
{
    [Fact]
    public void Should_NotDependOnResultNamespace_When_InDomainLayer()
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
}