using FluentAssertions;
using NetArchTest.Rules;
using Hector.BuildingBlocks.Domain.Primitives;
using System.Reflection;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public class StronglyTypedIdArchitectureTests
{
    private static readonly Assembly DomainAssembly =
        typeof(ValueObject).Assembly;

    [Fact]
    public void StronglyTypedId_Types_Should_Be_Sealed()
    {
        // Arrange
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(StronglyTypedIdCrtp<>))
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void StronglyTypedId_Types_Should_Reside_In_Primitives_Namespace()
    {
        // Arrange
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(StronglyTypedIdCrtp<>))
            .Should()
            .ResideInNamespace("Hector.BuildingBlocks.Domain.Primitives")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
