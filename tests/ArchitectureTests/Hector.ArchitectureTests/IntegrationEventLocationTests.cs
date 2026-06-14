using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.Modules.Projects.Contracts.Events;

namespace Hector.ArchitectureTests;

public sealed class IntegrationEventLocationTests
{
    [Fact]
    public void Should_ResideInContractsNamespace_When_TypeImplementsIIntegrationEvent()
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
            .Where(t => t.Namespace is null || !t.Namespace.Contains(".Contracts."))
            .Select(t => t.FullName)
            .ToList();

        // Assert
        invalidTypes.Should().BeEmpty(
            "Integration events must live in the module Contracts project/namespace as defined in ADR-0030.");
    }
}