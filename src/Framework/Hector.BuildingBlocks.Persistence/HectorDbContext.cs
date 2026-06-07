using System.Reflection;
using System.Text.Json;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Converters;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence;

public abstract class HectorDbContext : DbContext
{
    private readonly IStronglyTypedIdAssemblyProvider _stronglyTypedIdAssemblyProvider;

    protected HectorDbContext(
        DbContextOptions options,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider)
        : base(options)
    {
        _stronglyTypedIdAssemblyProvider = stronglyTypedIdAssemblyProvider;
    }

    // ✅ ثبت OutboxMessage در مدل
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.OccurredOn)
                .IsRequired();

            builder.Property(x => x.ProcessedOn)
                .IsRequired(false);
        });
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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEntities = ChangeTracker
            .Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(entity => entity.GetDomainEvents())
            .ToList();

        var outboxMessages = domainEvents
            .Select(domainEvent => new OutboxMessage
            {
                Id = Guid.NewGuid(),
                OccurredOn = domainEvent.OccurredOnUtc,
                Type = domainEvent.GetType().FullName!,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
            })
            .ToList();

        if (outboxMessages.Count > 0)
        {
            await OutboxMessages.AddRangeAsync(outboxMessages, cancellationToken);
        }

        // ✅ اول Commit انجام می‌شود
        var result = await base.SaveChangesAsync(cancellationToken);

        // ✅ فقط در صورت موفقیت، eventها پاک می‌شوند
        foreach (var entity in domainEntities)
        {
            entity.ClearDomainEvents();
        }

        return result;
    }

    private static bool IsConcreteStronglyTypedId(Type type)
    {
        if (type.IsAbstract || type.IsGenericTypeDefinition || !type.IsClass)
            return false;

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
