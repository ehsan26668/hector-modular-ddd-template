using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.ArchitectureTests;

public sealed class ModuleCompositionArchitectureTests
{
    [Fact]
    public void Every_Module_ShouldExpose_ExactlyOne_ModuleCompositionRoot()
    {
        // Arrange
        var moduleAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => a.GetName().Name!.StartsWith("Hector.Modules."))
            .ToList();

        // Act
        var modules = moduleAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IModule).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .ToList();

        // Assert
        modules.Should().NotBeEmpty(
            "Every module must expose exactly one module composition root according to ADR-0037.");

        modules
            .GroupBy(t => t.Assembly)
            .All(g => g.Count() == 1)
            .Should()
            .BeTrue("Each module must expose exactly one IModule implementation.");
    }
}
