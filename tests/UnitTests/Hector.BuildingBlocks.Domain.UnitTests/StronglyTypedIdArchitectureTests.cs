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
    public void StronglyTypedId_Concrete_Types_Should_Be_Sealed()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That()
            .Inherit(typeof(StronglyTypedId<>))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void StronglyTypedId_Base_Class_Should_Be_Abstract()
    {
        typeof(StronglyTypedId<>).IsAbstract.Should().BeTrue();
    }

    [Fact]
    public void StronglyTypedId_Should_Reside_In_Primitives_Namespace()
    {
        typeof(StronglyTypedId<>)
            .Namespace
            .Should()
            .Be("Hector.BuildingBlocks.Domain.Primitives");
    }
}
