using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Application.Messaging.Inbox;
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

    [Fact]
    public void IntegrationEvents_Should_Not_Implement_IInboxMessage()
    {
        // Arrange
        var assemblies = new[]
        {
            typeof(ProjectsContractsAssemblyMarker).Assembly
        };

        var integrationEventTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                typeof(IIntegrationEvent).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                !type.IsInterface)
            .ToArray();

        // Act
        var violations = integrationEventTypes
            .Where(type => typeof(IInboxMessage).IsAssignableFrom(type))
            .Select(type => type.FullName)
            .ToArray();

        // Assert
        violations.Should().BeEmpty(
            "integration events are external contracts, while inbox messages are consumer-specific persistence concerns");
    }

    [Fact]
    public void IntegrationEvents_Should_Not_Expose_Consumer_Property()
    {
        // Arrange
        var assemblies = new[]
        {
            typeof(ProjectsContractsAssemblyMarker).Assembly
        };

        var integrationEventTypes = assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                typeof(IIntegrationEvent).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                !type.IsInterface)
            .ToArray();

        // Act
        var violations = integrationEventTypes
            .Where(type => type.GetProperty("Consumer") is not null)
            .Select(type => type.FullName)
            .ToArray();

        // Assert
        violations.Should().BeEmpty(
            "consumer identity belongs to the subscriber or handler, not to the integration event contract");
    }
}
