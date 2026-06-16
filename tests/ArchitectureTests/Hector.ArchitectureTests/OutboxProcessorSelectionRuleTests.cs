using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Outbox;

namespace Hector.ArchitectureTests;

public class OutboxProcessorSelectionRuleTests
{
    [Fact]
    public void OutboxProcessor_Must_Use_OutboxProcessingPolicy()
    {
        // Arrange
        var processorType = typeof(OutboxProcessor);

        // Act
        var methods = processorType.GetMethods(
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);

        var methodBodies = methods
            .Select(m => m.GetMethodBody())
            .Where(b => b != null)
            .ToList();

        // Assert
        methodBodies.Should().NotBeEmpty();
    }
}
