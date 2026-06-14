using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Contracts.Events;

namespace Hector.ArchitectureTests;

public sealed class IntegrationEventNamingTests
{
    [Fact]
    public void Should_EndWithIntegrationEvent_When_TypeImplementsIIntegrationEvent()
    {
        // Arrange
        var assembly = typeof(ProjectCreatedIntegrationEvent).Assembly;

        var integrationEventTypes = assembly
            .GetTypes()
            .Where(t =>
                typeof(IIntegrationEvent).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .ToList();

        // Act
        var invalidTypes = integrationEventTypes
            .Where(t => !t.Name.EndsWith("IntegrationEvent", StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ToList();

        // Assert
        invalidTypes.Should().BeEmpty(
            "All integration events must use the 'IntegrationEvent' suffix as defined in ADR-0030.");
    }
}