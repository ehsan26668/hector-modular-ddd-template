using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using static Hector.Persistence.Testing.PersistenceTestInfrastructure;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class HectorDbContextTests
{
    [Fact]
    public async Task Should_DispatchDomainEvents_When_AggregateRaisesDomainEvent()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        var domainEventDispatcher = new RecordingDomainEventDispatcher();

        await using var context = await CreateContextAsync(connection, domainEventDispatcher);

        var aggregate = TestAggregate.Create();
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        domainEventDispatcher.DispatchedEvents.Should().ContainSingle();
    }

    [Fact]
    public async Task Should_ClearDomainEvents_When_SaveChangesSucceeds()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateContextAsync(connection);

        var aggregate = TestAggregate.Create();
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        ((IHasDomainEvents)aggregate).GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public async Task Should_NotClearDomainEvents_When_PersistenceFails()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateFailingContextAsync(connection);

        var aggregate = TestAggregate.Create();
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        Func<Task> act = () => context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
        ((IHasDomainEvents)aggregate).GetDomainEvents().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Should_NotCommitAggregateChanges_When_PersistenceFails()
    {
        // Arrange
        using var connection = CreateOpenSqliteConnection();
        await using var context = await CreateFailingContextAsync(connection);

        var aggregate = TestAggregate.Create();
        context.Add(aggregate);

        // Act
        Func<Task> act = () => context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();

        var count = await context.TestAggregates.CountAsync();
        count.Should().Be(0);
    }
}
