using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.ArchitectureTests;

public sealed class ModuleCompositionArchitectureTests
{
    [Fact]
    public void Every_Module_ShouldExpose_ExactlyOne_ModuleCompositionRoot()
    {
        // Arrange
        var moduleInfrastructureAssemblies = Directory
            .EnumerateFiles(AppContext.BaseDirectory, "Hector.Modules.*.Infrastructure.dll")
            .Select(Assembly.LoadFrom)
            .ToList();

        // Act
        var moduleCompositionRoots = moduleInfrastructureAssemblies
            .Select(assembly => new
            {
                Assembly = assembly,
                Modules = assembly
                    .GetTypes()
                    .Where(type =>
                        typeof(IModule).IsAssignableFrom(type) &&
                        !type.IsAbstract &&
                        !type.IsInterface)
                    .ToList()
            })
            .ToList();

        // Assert
        moduleInfrastructureAssemblies.Should().NotBeEmpty(
            "ADR-0037 requires every module infrastructure assembly to expose one composition root.");

        moduleCompositionRoots
            .Where(result => result.Modules.Count != 1)
            .Should()
            .BeEmpty("each module infrastructure assembly must expose exactly one IModule implementation.");
    }
}
