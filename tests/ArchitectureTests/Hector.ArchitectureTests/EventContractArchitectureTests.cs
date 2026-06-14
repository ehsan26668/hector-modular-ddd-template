using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.Modules.Projects.Contracts;

namespace Hector.ArchitectureTests;

public sealed class EventContractArchitectureTests
{
    [Fact]
    public void Should_HaveUniqueContractIdentity_When_IntegrationEventsAreDefined()
    {
        // Arrange
        var assemblies = new[]
        {
            typeof(ProjectsContractsAssemblyMarker).Assembly
        };

        var integrationEventTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IIntegrationEvent).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .ToList();

        var contracts = new Dictionary<(string Name, int Version), Type>();

        // Act
        foreach (var type in integrationEventTypes)
        {
            var attribute = type.GetCustomAttribute<OutboxEventAttribute>();

            attribute.Should().NotBeNull(
                $"Integration event '{type.FullName}' must declare OutboxEventAttribute.");

            var key = (attribute!.Name, attribute.Version);

            if (contracts.TryGetValue(key, out var existingType))
            {
                throw new InvalidOperationException(
                    $"Duplicate integration event contract detected: '{attribute.Name}' v{attribute.Version} " +
                    $"defined on '{type.FullName}' and '{existingType.FullName}'.");
            }

            contracts[key] = type;
        }

        // Assert
        contracts.Should().NotBeEmpty();
    }
}
