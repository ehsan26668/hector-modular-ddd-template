namespace Hector.BuildingBlocks.Domain.Primitives;

public interface IHasDomainEvents
{
    IReadOnlyCollection<DomainEventBase> GetDomainEvents();

    void ClearDomainEvents();
}