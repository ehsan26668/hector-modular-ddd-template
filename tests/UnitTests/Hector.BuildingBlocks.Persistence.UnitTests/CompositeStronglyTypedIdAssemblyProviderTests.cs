using System.Reflection;
using FluentAssertions;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class CompositeStronglyTypedIdAssemblyProviderTests
{
    [Fact]
    public void Should_ReturnUnionOfAssemblies_FromAllProviders()
    {
        // Arrange
        var assembly = Assembly.GetExecutingAssembly();

        var provider1 = new FakeProvider(assembly);
        var provider2 = new FakeProvider(assembly);

        var composite = new CompositeStronglyTypedIdAssemblyProvider(
            [provider1, provider2]);

        // Act
        var assemblies = composite.GetAssemblies();

        // Assert
        assemblies.Should().ContainSingle();
        assemblies.Should().Contain(assembly);
    }

    private sealed class FakeProvider(
        Assembly assembly)
        : IStronglyTypedIdAssemblyProvider
    {
        public FakeProvider() : this(Assembly.GetExecutingAssembly()) { }

        public IReadOnlyCollection<Assembly> GetAssemblies()
            => [assembly];
    }
}