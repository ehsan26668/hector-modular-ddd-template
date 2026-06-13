using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.ArchitectureTests;

public sealed class EventContractSnapshotTests
{
    [Fact]
    public void EventContracts_Should_Match_Snapshot()
    {
        // Arrange
        var snapshotPath = Path.Combine(
            AppContext.BaseDirectory,
            "event-contracts.snapshot");

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var contracts = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                typeof(IDomainEvent).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface)
            .Select(t => new
            {
                Type = t,
                Attribute = t.GetCustomAttribute<OutboxEventAttribute>()
            })
            .Where(x => x.Attribute is not null)
            .Select(x =>
                $"{x.Attribute!.Name}:v{x.Attribute!.Version}")
            .OrderBy(x => x)
            .ToList();

        var actual = string.Join(Environment.NewLine, contracts);

        // Act
        var expected = File.ReadAllText(snapshotPath).Trim();

        // Assert
        actual.Should().Be(expected);
    }
}
