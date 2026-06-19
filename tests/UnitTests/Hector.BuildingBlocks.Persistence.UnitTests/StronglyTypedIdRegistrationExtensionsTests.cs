using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class StronglyTypedIdRegistrationExtensionsTests
{
    [Fact]
    public void Should_RegisterCompositeProvider_When_ModuleAssemblyContainsProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddStronglyTypedIdInfrastructure(
            typeof(TestStronglyTypedIdAssemblyProvider).Assembly);

        using var provider = services.BuildServiceProvider();

        // Act
        var assemblyProvider =
            provider.GetRequiredService<IStronglyTypedIdAssemblyProvider>();

        // Assert
        assemblyProvider.Should().BeOfType<CompositeStronglyTypedIdAssemblyProvider>();
    }

    [Fact]
    public void Should_DiscoverAssemblies_FromRegisteredProviders()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddStronglyTypedIdInfrastructure(
            typeof(TestStronglyTypedIdAssemblyProvider).Assembly);

        using var provider = services.BuildServiceProvider();

        // Act
        var compositeProvider =
            provider.GetRequiredService<IStronglyTypedIdAssemblyProvider>();

        var assemblies = compositeProvider.GetAssemblies();

        // Assert
        // Now we are checking if the assembly of our REAL fake strongly typed id is included
        assemblies.Should().Contain(typeof(FakeProjectId).Assembly);
    }

    [Fact]
    public void Should_NotThrow_When_ResolvingCompositeProvider()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddStronglyTypedIdInfrastructure(
            typeof(TestStronglyTypedIdAssemblyProvider).Assembly);

        using var provider = services.BuildServiceProvider();

        // Act
        Action act = () =>
            provider.GetRequiredService<IStronglyTypedIdAssemblyProvider>();

        // Assert
        act.Should().NotThrow(
            "Composite provider must resolve without circular dependencies.");
    }

    // This class acts as a concrete StronglyTypedId type for testing purposes.
    // It simulates a real ID like ProjectId without depending on the Projects module.
    private sealed class TestStronglyTypedIdAssemblyProvider
        : IStronglyTypedIdAssemblyProvider
    {
        public IReadOnlyCollection<Assembly> GetAssemblies()
            => [typeof(FakeProjectId).Assembly]; // Pointing to our fake ID's assembly
    }

    // This is the actual fake StronglyTypedId we use for testing.
    private sealed class FakeProjectId : StronglyTypedId<FakeProjectId>
    {
        public FakeProjectId(Guid value) : base(value) { }
    }
}
