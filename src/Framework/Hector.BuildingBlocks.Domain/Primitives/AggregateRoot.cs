namespace Hector.BuildingBlocks.Domain.Primitives;

public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<DomainEventBase> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id)
    {
    }

    IReadOnlyCollection<DomainEventBase> IHasDomainEvents.GetDomainEvents()
    {
        return _domainEvents.AsReadOnly();
    }

    protected void RaiseDomainEvent(DomainEventBase domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}