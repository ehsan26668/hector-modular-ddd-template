using NetArchTest.Rules;

namespace Hector.ArchitectureTests;

public sealed class QueryHandlerSideEffectTests
{
    [Fact]
    public void Should_NotImplementCommandHandler_When_ClassImplementsQueryHandler()
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
                    .ShouldNot()
                    .ImplementInterface(typeof(Hector.BuildingBlocks.Application.Messaging.ICommandHandler<,>))
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
            $"QueryHandlers must not implement command handlers. Failed assemblies: {string.Join(", ", failures)}");
    }
}