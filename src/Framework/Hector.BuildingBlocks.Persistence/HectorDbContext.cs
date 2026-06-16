using System.Reflection;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Converters;
using Hector.BuildingBlocks.Persistence.Inbox;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hector.BuildingBlocks.Persistence;

public abstract class HectorDbContext(
    DbContextOptions options,
    IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider,
    IDomainEventDispatcher domainEventDispatcher)
    : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        var assemblies = stronglyTypedIdAssemblyProvider
            .GetAssemblies()
            .Distinct()
            .ToList();

        var stronglyTypedIdTypes = assemblies
            .SelectMany(GetLoadableTypes)
            .Where(IsConcreteStronglyTypedId)
            .ToList();

        foreach (var stronglyTypedIdType in stronglyTypedIdTypes)
        {
            var converterType = typeof(StronglyTypedIdValueConverter<>).MakeGenericType(stronglyTypedIdType);
            configurationBuilder.Properties(stronglyTypedIdType).HaveConversion(converterType);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HectorDbContext).Assembly);

        modelBuilder.Entity<OutboxMessage>(ConfigureOutboxMessage);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.GetDomainEvents().Count != 0)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(entity => entity.GetDomainEvents())
            .ToList();

        if (domainEvents.Count > 0)
        {
            await domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var domainEntity in domainEntities)
        {
            domainEntity.ClearDomainEvents();
        }

        return result;
    }

    private static void ConfigureOutboxMessage(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.OccurredOn).IsRequired();
        builder.Property(x => x.ProcessedOn).IsRequired(false);
        builder.Property(x => x.Error).IsRequired(false);
        builder.Property(x => x.LastAttemptedOn).IsRequired(false);
        builder.Property(x => x.LockedUntil).IsRequired(false);
        builder.Property(x => x.LockId).IsRequired(false);
    }

    private static bool IsConcreteStronglyTypedId(Type type)
    {
        return type is { IsAbstract: false, IsInterface: false }
            && InheritsFromStronglyTypedId(type);
    }

    private static bool InheritsFromStronglyTypedId(Type type)
    {
        var currentType = type.BaseType;

        while (currentType is not null)
        {
            if (currentType.IsGenericType &&
                currentType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }
}
