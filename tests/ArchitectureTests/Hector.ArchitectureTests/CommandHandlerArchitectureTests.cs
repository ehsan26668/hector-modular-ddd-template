using NetArchTest.Rules;

namespace Hector.ArchitectureTests;

public sealed class CommandHandlerArchitectureTests
{
    [Fact]
    public void Should_DependOnDomainLayer_When_ClassImplementsCommandHandler()
    {
        // Arrange
        var applicationAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly =>
                assembly.GetName().Name is not null &&
                assembly.GetName().Name!.Contains(".Application"));

        // Act
        var results = applicationAssemblies
            .Select(assembly => new
            {
                AssemblyName = assembly.GetName().Name!,
                Result = Types
                    .InAssembly(assembly)
                    .That()
                    .ImplementInterface(typeof(Hector.BuildingBlocks.Application.Messaging.ICommandHandler<,>))
                    .Should()
                    .HaveDependencyOn("Domain")
                    .GetResult()
            })
            .ToList();

        // Assert
        var failures = results
            .Where(x => !x.Result.IsSuccessful)
            .Select(x => x.AssemblyName)
            .ToList();

        Assert.True(
            failures.Count == 0,
            $"CommandHandlers should depend on Domain layer. Failed assemblies: {string.Join(", ", failures)}");
    }

    [Fact]
    public void Should_NotDependOnInfrastructureLayer_When_ClassImplementsCommandHandler()
    {
        // Arrange
        var applicationAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly =>
                assembly.GetName().Name is not null &&
                assembly.GetName().Name!.Contains(".Application"));

        // Act
        var results = applicationAssemblies
            .Select(assembly => new
            {
                AssemblyName = assembly.GetName().Name!,
                Result = Types
                    .InAssembly(assembly)
                    .That()
                    .ImplementInterface(typeof(Hector.BuildingBlocks.Application.Messaging.ICommandHandler<,>))
                    .ShouldNot()
                    .HaveDependencyOn("Infrastructure")
                    .GetResult()
            })
            .ToList();

        // Assert
        var failures = results
            .Where(x => !x.Result.IsSuccessful)
            .Select(x => x.AssemblyName)
            .ToList();

        Assert.True(
            failures.Count == 0,
            $"CommandHandlers should not depend on Infrastructure layer. Failed assemblies: {string.Join(", ", failures)}");
    }
}