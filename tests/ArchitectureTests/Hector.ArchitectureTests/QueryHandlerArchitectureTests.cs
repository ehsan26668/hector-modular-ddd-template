using NetArchTest.Rules;

namespace Hector.ArchitectureTests;

public sealed class QueryHandlerArchitectureTests
{
    [Fact]
    public void Should_NotDependOnDomainLayer_When_ClassImplementsQueryHandler()
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
                    .ImplementInterface(typeof(Hector.BuildingBlocks.Application.Messaging.IQueryHandler<,>))
                    .ShouldNot()
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
            $"QueryHandlers should not depend on Domain layer. Failed assemblies: {string.Join(", ", failures)}");
    }
}
