using NetArchTest.Rules;

namespace Hector.ArchitectureTests;

public sealed class QueryHandlerDbContextUsageTests
{
    [Fact]
    public void Should_DependOnDbContext_When_ClassImplementsQueryHandler()
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
                    .ImplementInterface(typeof(Hector.BuildingBlocks.Application.Messaging.IQueryHandler<,>))
                    .Should()
                    .HaveDependencyOn("DbContext")
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
            $"QueryHandlers should use DbContext for read models. Failed assemblies: {string.Join(", ", failures)}");
    }
}