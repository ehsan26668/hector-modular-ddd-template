using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public class AggregateRootTests
{
    [Fact]
    public void RaiseDomainEvent_Should_Add_Event_To_Collection()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();

        // Act
        aggregate.Raise(domainEvent);

        // Assert
        aggregate.GetDomainEvents().Should().Contain(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_Should_Remove_All_Events()
    {
        // Arrange
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestDomainEvent();

        aggregate.Raise(domainEvent);

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.GetDomainEvents().Should().BeEmpty();
    }

    internal sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id) : base(id) { }

        public void Raise(IDomainEvent domainEvent)
        {
            RaiseDomainEvent(domainEvent);
        }
    }

    internal record TestDomainEvent : IDomainEvent;
}
