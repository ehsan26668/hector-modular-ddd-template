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
    public void Should_NotImplementIInboxMessage_When_AnalyzingIntegrationEvents()
    {
        // Arrange
        var integrationEventAssemblies = LoadIntegrationEventAssemblies();

        // Act
        var failures = integrationEventAssemblies
            .SelectMany(FindIntegrationEventsImplementingIInboxMessage)
            .ToList();

        // Assert
        failures.Should().BeEmpty(
            "integration events must not implement IInboxMessage.{0}{1}",
            Environment.NewLine,
            FormatFailures(failures));
    }

    [Fact]
    public void Should_NotExposeConsumerProperty_When_AnalyzingIntegrationEvents()
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

    private static IReadOnlyCollection<Assembly> LoadIntegrationEventAssemblies()
    {
        return Directory
            .EnumerateFiles(
                AppContext.BaseDirectory,
                "Hector.Modules.*.Contracts.dll",
                SearchOption.TopDirectoryOnly)
            .OrderBy(static path => path)
            .Select(Assembly.LoadFrom)
            .ToList();
    }

    private static IEnumerable<string> FindIntegrationEventsImplementingIInboxMessage(
        Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(type =>
                typeof(IIntegrationEvent).IsAssignableFrom(type) &&
                typeof(IInboxMessage).IsAssignableFrom(type) &&
                !type.IsAbstract &&
                !type.IsInterface)
            .Select(type => type.FullName!)
            .Distinct();
    }

    private static string FormatFailures(IEnumerable<string> failures)
    {
        var formattedFailures = failures.ToList();

        return formattedFailures.Count == 0
            ? string.Empty
            : string.Join(
                Environment.NewLine,
                formattedFailures.Select(static failure => $"  - {failure}"));
    }
}
