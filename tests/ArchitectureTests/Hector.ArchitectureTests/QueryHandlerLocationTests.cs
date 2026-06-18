using NetArchTest.Rules;
using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.ArchitectureTests;

public sealed class QueryHandlerLocationTests
{
    [Fact]
    public void Should_ResideInQueriesNamespace_When_ClassImplementsQueryHandler()
    {
        // Arrange
        var applicationAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a =>
                a.GetName().Name is not null &&
                a.GetName().Name!.Contains(".Application"));

        // Act
        var results = applicationAssemblies
            .Select(assembly => new
            {
                AssemblyName = assembly.GetName().Name!,
                Result = Types
                    .InAssembly(assembly)
                    .That()
                    .ImplementInterface(typeof(IQueryHandler<,>))
                    .Should()
                    .ResideInNamespaceMatching(".*\\.Application\\.Queries.*")
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
            $"QueryHandlers must reside in Application.Queries namespace. Failed assemblies: {string.Join(", ", failures)}");
    }
}
