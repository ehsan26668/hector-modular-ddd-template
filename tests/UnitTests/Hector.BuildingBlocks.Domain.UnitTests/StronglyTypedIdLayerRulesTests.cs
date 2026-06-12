using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public class StronglyTypedIdLayerRulesTests
{
    [Fact]
    public void Should_ResideInDomainAssembly_When_TypeInheritsFrom_StronglyTypedId()
    {
        // Arrange
        var stronglyTypedIdTypes = GetConcreteStronglyTypedIdTypes();

        // Act
        var invalidTypes = stronglyTypedIdTypes
            .Where(type => !type.Assembly.GetName().Name!.Contains(".Domain"))
            .ToArray();

        // Assert
        invalidTypes.Should().BeEmpty(
            "StronglyTypedId types must be defined inside Domain assemblies only.");
    }

    [Fact]
    public void Should_ResideInDomainNamespace_When_TypeInheritsFrom_StronglyTypedId()
    {
        // Arrange
        var stronglyTypedIdTypes = GetConcreteStronglyTypedIdTypes();

        // Act
        var invalidTypes = stronglyTypedIdTypes
            .Where(type => type.Namespace is null || !type.Namespace.Contains(".Domain"))
            .ToArray();

        // Assert
        invalidTypes.Should().BeEmpty(
            "StronglyTypedId types must live inside Domain namespaces.");
    }

    private static Type[] GetConcreteStronglyTypedIdTypes()
    {
        return [.. AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type =>
                type is { IsAbstract: false, IsClass: true } &&
                type.BaseType is not null &&
                type.BaseType.IsGenericType &&
                type.BaseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))];
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
