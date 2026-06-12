using System.Reflection;
using FluentAssertions;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class StronglyTypedIdAssemblyProviderArchitectureTests
{
    [Fact]
    public void Should_HaveExactlyOneStronglyTypedIdAssemblyProvider_PerModuleInfrastructureAssembly()
    {
        // Arrange
        var infrastructureAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(static assembly =>
                !assembly.IsDynamic &&
                assembly.GetName().Name is not null &&
                assembly.GetName().Name!.StartsWith("Hector.Modules.") &&
                assembly.GetName().Name!.EndsWith(".Infrastructure"))
            .ToArray();

        // Act
        var violations = infrastructureAssemblies
            .Select(static assembly => new
            {
                AssemblyName = assembly.GetName().Name!,
                ProviderTypes = GetLoadableTypes(assembly)
                    .Where(static type =>
                        type is { IsAbstract: false, IsInterface: false } &&
                        typeof(IStronglyTypedIdAssemblyProvider).IsAssignableFrom(type))
                    .ToArray()
            })
            .Where(static x => x.ProviderTypes.Length != 1)
            .ToArray();

        // Assert
        violations.Should().BeEmpty(
            "each module infrastructure assembly must define exactly one strongly typed id assembly provider.");
    }

    private static IReadOnlyCollection<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return [.. exception.Types
                .Where(static type => type is not null)
                .Cast<Type>()];
        }
    }
}
