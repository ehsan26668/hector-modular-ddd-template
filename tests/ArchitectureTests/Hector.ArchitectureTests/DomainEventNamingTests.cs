using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.Modules.Projects.Domain;

namespace Hector.ArchitectureTests;

public sealed class DomainEventNamingTests
{
    [Fact]
    public void Should_EndWithDomainEvent_When_TypeImplementsIDomainEvent()
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
            .Where(t => !t.Name.EndsWith("DomainEvent", StringComparison.Ordinal))
            .Select(t => t.FullName)
            .ToList();

        // Assert
        invalidTypes.Should().BeEmpty(
            "All domain events must use the 'DomainEvent' suffix as defined in ADR-0030.");
    }
}