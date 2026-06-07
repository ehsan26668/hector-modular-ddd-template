using System.Reflection;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence;

public abstract class HectorDbContext : DbContext
{
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly IStronglyTypedIdAssemblyProvider _stronglyTypedIdAssemblyProvider;

    protected HectorDbContext(
        DbContextOptions options,
        IDomainEventDispatcher domainEventDispatcher,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider)
        : base(options)
    {
        _domainEventDispatcher = domainEventDispatcher;
        _stronglyTypedIdAssemblyProvider = stronglyTypedIdAssemblyProvider;
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        var stronglyTypedIdTypes = _stronglyTypedIdAssemblyProvider
            .GetAssemblies()
            .Distinct()
            .SelectMany(GetLoadableTypes)
            .Where(IsConcreteStronglyTypedId)
            .ToArray();

        foreach (var stronglyTypedIdType in stronglyTypedIdTypes)
        {
            var converterType = typeof(StronglyTypedIdValueConverter<>)
                .MakeGenericType(stronglyTypedIdType);

            configurationBuilder
                .Properties(stronglyTypedIdType)
                .HaveConversion(converterType);
        }
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var domainEventEntries = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.GetDomainEvents().Count > 0)
            .ToArray();

        var domainEvents = domainEventEntries
            .SelectMany(entry => entry.Entity.GetDomainEvents())
            .ToArray();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (domainEvents.Length == 0)
        {
            return result;
        }

        await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);

        foreach (var entry in domainEventEntries)
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }

    private static bool IsConcreteStronglyTypedId(Type type)
    {
        if (type.IsAbstract || type.IsGenericTypeDefinition || !type.IsClass)
        {
            return false;
        }

        var current = type.BaseType;

        while (current is not null)
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(StronglyTypedId<>))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null)!;
        }
    }
}
