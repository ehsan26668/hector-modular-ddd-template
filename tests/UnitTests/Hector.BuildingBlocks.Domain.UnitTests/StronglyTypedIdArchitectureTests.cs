using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public class StronglyTypedIdArchitectureTests
{
    [Fact]
    public void Should_BeAbstract_When_Checking_StronglyTypedId_BaseType()
    {
        // Arrange
        var stronglyTypedIdType = typeof(StronglyTypedId<>);

        // Act
        var isAbstract = stronglyTypedIdType.IsAbstract;

        // Assert
        isAbstract.Should().BeTrue(
            "StronglyTypedId is designed as an abstract base type.");
    }

    [Fact]
    public void Should_ResideIn_PrimitivesNamespace_When_Checking_StronglyTypedId_BaseType()
    {
        // Arrange
        var stronglyTypedIdType = typeof(StronglyTypedId<>);

        // Act
        var namespaceName = stronglyTypedIdType.Namespace;

        // Assert
        namespaceName.Should().Be(
            "Hector.BuildingBlocks.Domain.Primitives",
            "StronglyTypedId must reside in the Domain.Primitives namespace.");
    }

    [Fact]
    public void Should_BeSealed_When_TypeInheritsFrom_StronglyTypedId()
    {
        // Arrange
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // Act
        var result = Types.InAssemblies(assemblies)
            .That()
            .Inherit(typeof(StronglyTypedId<>))
            .And()
            .AreNotAbstract()
            .Should()
            .BeSealed()
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Concrete StronglyTypedId types must be sealed to preserve value semantics.");
    }

    [Fact]
    public void Should_HaveNonPublicGuidConstructor_When_TypeInheritsFrom_StronglyTypedId()
    {
        // Arrange
        var stronglyTypedIdTypes = GetConcreteStronglyTypedIdTypes();

        // Act
        var invalidTypes = stronglyTypedIdTypes
            .Where(type =>
                type.GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    binder: null,
                    types: new[] { typeof(Guid) },
                    modifiers: null) is null)
            .ToArray();

        // Assert
        invalidTypes.Should().BeEmpty(
            "EF Core StronglyTypedIdValueConverter requires a non-public Guid constructor.");
    }

    private static Type[] GetConcreteStronglyTypedIdTypes()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type =>
                type is { IsAbstract: false, IsClass: true } &&
                type.BaseType is not null &&
                type.BaseType.IsGenericType &&
                type.BaseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))
            .ToArray();
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
