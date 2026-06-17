using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;

namespace Hector.ArchitectureTests;

public sealed class ModuleIdentityArchitectureTests
{
    [Fact]
    public void Every_Module_ShouldExpose_ExactlyOne_ModuleIdentity()
    {
        // Arrange
        var moduleAssemblies = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(a => a.GetName().Name!.StartsWith("Hector.Modules."))
            .ToList();

        // Act
        var identities = moduleAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IModuleIdentity).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .ToList();

        // Assert
        identities.Should().NotBeEmpty(
            "Every module must expose exactly one module identity according to ADR-0037.");

        identities
            .GroupBy(t => t.Assembly)
            .All(g => g.Count() == 1)
            .Should()
            .BeTrue("Each module must expose exactly one IModuleIdentity.");
    }
}
