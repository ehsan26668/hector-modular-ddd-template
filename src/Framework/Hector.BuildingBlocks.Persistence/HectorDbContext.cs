using Hector.BuildingBlocks.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence;

public abstract class HectorDbContext : DbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    protected HectorDbContext(
        DbContextOptions options,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is IHasDomainEvents)
            .Select(entry => (IHasDomainEvents)entry.Entity)
            .SelectMany(entity => entity.GetDomainEvents())
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (domainEvents.Length > 0)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        ClearDomainEvents();

        return result;
    }

    private void ClearDomainEvents()
    {
        var entities = ChangeTracker
            .Entries()
            .Where(entry => entry.Entity is IHasDomainEvents)
            .Select(entry => entry.Entity)
            .Cast<IHasDomainEvents>();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }
}
