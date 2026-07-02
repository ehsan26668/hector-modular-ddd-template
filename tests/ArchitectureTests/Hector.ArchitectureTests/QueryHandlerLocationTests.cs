using NetArchTest.Rules;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.ArchitectureTests.Common;

namespace Hector.ArchitectureTests;

public sealed class QueryHandlerLocationTests : ArchitectureTestBase
{
    [Fact]
    public void Should_ResideInQueriesNamespace_When_ClassImplementsQueryHandler()
    {
        // Arrange
        var assemblies = ApplicationAssemblies;

        // Act
        var results = assemblies
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
