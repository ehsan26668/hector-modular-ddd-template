using FluentAssertions;
using Hector.Testing.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxPersistenceTests
{
    [Fact]
    public async Task Should_PersistOutboxMessage_When_AggregateContainsDomainEvent()
    {
        // Arrange
        using var connection = PersistenceTestInfrastructure.CreateOpenSqliteConnection();
        await using var context = await PersistenceTestInfrastructure.CreateContextAsync(connection);

        // We use the TestAggregate defined in our shared persistence infrastructure
        var aggregate = PersistenceTestInfrastructure.TestAggregate.Create();
        aggregate.RaiseTestEvent(); // This ensures at least one domain event is raised

        context.TestAggregates.Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var messages = await context.OutboxMessages.ToListAsync();

        messages.Should().HaveCount(1);
        messages[0].Type.Should().Be(typeof(PersistenceTestInfrastructure.TestDomainEvent).AssemblyQualifiedName);
        messages[0].ProcessedOn.Should().BeNull();
    }
}
