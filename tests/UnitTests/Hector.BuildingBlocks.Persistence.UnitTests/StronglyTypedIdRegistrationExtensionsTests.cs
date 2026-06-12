using System.Reflection;
using FluentAssertions;
using Hector.Modules.Projects.Domain;
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
        assemblies.Should().Contain(typeof(ProjectId).Assembly);
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

    private sealed class TestStronglyTypedIdAssemblyProvider
        : IStronglyTypedIdAssemblyProvider
    {
        public IReadOnlyCollection<Assembly> GetAssemblies()
            => [typeof(ProjectId).Assembly];
    }
}
