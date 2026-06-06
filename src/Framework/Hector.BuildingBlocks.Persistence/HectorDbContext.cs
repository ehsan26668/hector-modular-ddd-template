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
            .Entries<IHasDomainEvents>()
            .SelectMany(entry => entry.Entity.GetDomainEvents())
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (domainEvents.Length > 0)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        foreach (var entry in ChangeTracker.Entries<IHasDomainEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }

    private void ClearDomainEvents()
    {
        foreach (var entry in ChangeTracker.Entries<IHasDomainEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }
    }
}
