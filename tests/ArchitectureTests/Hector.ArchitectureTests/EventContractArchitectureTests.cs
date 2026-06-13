using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Domain;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.Modules.Projects.Domain;

namespace Hector.ArchitectureTests;

public sealed class EventContractArchitectureTests
{
    [Fact]
    public void DomainEvents_Should_Have_Unique_EventContract()
    {
        // Arrange
        var assemblies = new[]
        {
            typeof(DomainAssemblyMarker).Assembly,
            typeof(ProjectsDomainAssemblyMarker).Assembly
        };

        var domainEventTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IDomainEvent).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .ToList();

        var contracts = new Dictionary<(string Name, int Version), Type>();

        // Act
        foreach (var type in domainEventTypes)
        {
            var attribute = type.GetCustomAttribute<OutboxEventAttribute>();

            if (attribute is null)
                continue;

            var key = (attribute.Name, attribute.Version);

            if (contracts.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"Duplicate event contract detected: '{attribute.Name}' v{attribute.Version} " +
                    $"defined on '{type.FullName}' and '{contracts[key].FullName}'.");
            }

            contracts[key] = type;
        }

        // Assert
        contracts.Should().NotBeEmpty();
    }
}
