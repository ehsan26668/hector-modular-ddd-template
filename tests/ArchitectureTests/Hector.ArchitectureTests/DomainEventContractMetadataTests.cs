using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.Modules.Projects.Domain;

namespace Hector.ArchitectureTests;

public sealed class DomainEventContractMetadataTests
{
    [Fact]
    public void Should_NotDeclareOutboxMetadata_When_TypeIsDomainEvent()
    {
        // Arrange
        var assembly = typeof(ProjectsDomainAssemblyMarker).Assembly;

        var domainEventTypes = assembly
            .GetTypes()
            .Where(t =>
                typeof(IDomainEvent).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .ToList();

        // Act
        var invalidTypes = domainEventTypes
            .Where(t => t.GetCustomAttribute<OutboxEventAttribute>() is not null)
            .Select(t => t.FullName)
            .ToList();

        // Assert
        invalidTypes.Should().BeEmpty(
            "Domain events must not carry OutboxEventAttribute because transport contract metadata belongs only to integration events.");
    }
}