using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Application.Messaging;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.Modules.Projects.Contracts.Events;

namespace Hector.ArchitectureTests;

public sealed class EventContractSnapshotTests
{
    [Fact]
    public void Should_MatchSnapshot_When_IntegrationEventContractsAreEnumerated()
    {
        // Arrange
        var snapshotPath = Path.Combine(
            AppContext.BaseDirectory,
            "event-contracts.snapshot");

        var assembly = typeof(ProjectCreatedIntegrationEvent).Assembly;

        var contracts = assembly
            .GetTypes()
            .Where(t =>
                typeof(IIntegrationEvent).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<OutboxEventAttribute>()
            })
            .Where(x => x.Attribute is not null)
            .Select(x => $"{x.Attribute!.Name}:v{x.Attribute!.Version}")
            .OrderBy(x => x)
            .ToList();

        var actual = string.Join(Environment.NewLine, contracts);
        var expected = File.ReadAllText(snapshotPath).Trim();

        // Act
        var result = actual;

        // Assert
        result.Should().Be(expected);
    }
}
