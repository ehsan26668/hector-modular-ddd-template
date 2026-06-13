using FluentAssertions;
using Hector.Testing.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxPersistenceTests
{
    private const string EventName = "test.persistence-domain-event";
    private const int EventVersion = 1;

    [Fact]
    public async Task Should_PersistOutboxMessage_When_AggregateContainsDomainEvent()
    {
        // Arrange
        using var connection = PersistenceTestInfrastructure.CreateOpenSqliteConnection();
        await using var context = await PersistenceTestInfrastructure.CreateContextAsync(connection);

        var aggregate = PersistenceTestInfrastructure.TestAggregate.Create();
        aggregate.RaiseTestEvent();

        context.TestAggregates.Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var messages = await context.OutboxMessages.ToListAsync();

        messages.Should().HaveCount(1);
        messages[0].Type.Should().Be(EventName);
        messages[0].Version.Should().Be(EventVersion);
        messages[0].ProcessedOn.Should().BeNull();
    }
}
