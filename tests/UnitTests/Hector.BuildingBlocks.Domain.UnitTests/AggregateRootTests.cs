using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class AggregateRootTests
{
    [Fact]
    public void Should_AddDomainEvent_ToCollection_When_ActionIsPerformed()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();

        // Act
        aggregate.Trigger(domainEvent);

        // Assert
        ((IHasDomainEvents)aggregate)
            .GetDomainEvents()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(domainEvent);
    }

    [Fact]
    public void Should_ClearDomainEvents_When_ClearIsCalled()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();

        aggregate.Trigger(domainEvent);

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        ((IHasDomainEvents)aggregate)
            .GetDomainEvents()
            .Should()
            .BeEmpty();
    }

    private sealed class TestAggregate(Guid id) : AggregateRoot<Guid>(id)
    {
        public void Trigger(TestDomainEvent domainEvent)
        {
            RaiseDomainEvent(domainEvent);
        }
    }

    private sealed record TestDomainEvent : DomainEventBase;
}
