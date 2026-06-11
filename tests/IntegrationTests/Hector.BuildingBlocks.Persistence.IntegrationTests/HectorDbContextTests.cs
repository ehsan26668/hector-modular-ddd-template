using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using static Hector.Testing.Persistence.PersistenceTestInfrastructure;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class HectorDbContextTests
{
    [Fact]
    public async Task Should_PersistOutboxMessage_When_AggregateRaisesDomainEvent()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = await CreateContextAsync(connection);

        var aggregate = TestAggregate.Create();
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();

        outboxMessages.Should().ContainSingle();
        outboxMessages[0].Type.Should().Be(typeof(TestDomainEvent).AssemblyQualifiedName);
        outboxMessages[0].Content.Should().Contain(aggregate.Id.Value.ToString());
    }

    [Fact]
    public async Task Should_ClearDomainEvents_When_SaveChangesSucceeds()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
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
        using var connection = CreateOpenInMemoryConnection();
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
    public async Task Should_NotPersistOutboxMessage_When_PersistenceFails()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = await CreateFailingContextAsync(connection);

        var aggregate = TestAggregate.Create();
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        Func<Task> act = () => context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();

        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();
        outboxMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task Should_NotCommitAggregateChanges_WhenPeresistenceFails()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = await CreateFailingContextAsync(connection);

        var aggregate = TestAggregate.Create();
        context.Add(aggregate);

        // Act
        Func<Task> act = () => context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();

        // Check if the record exists in DB (should be 0)
        var count = await context.TestAggregates.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task Should_DispatchDomainEvents_When_SaveChangesSucceeds()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        var dispatcher = new RecordingDomainEventDispatcher();
        await using var context = await CreateContextAsync(connection, dispatcher);

        var aggregate = TestAggregate.Create();
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        dispatcher.DispatchedEvents.Should().ContainSingle();
        dispatcher.DispatchedEvents[0].Should().BeOfType<TestDomainEvent>();
        ((TestDomainEvent)dispatcher.DispatchedEvents[0]).AggregateId.Should().Be(aggregate.Id.Value);
    }
}
