using Hector.BuildingBlocks.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public class HectorDbContextTests
{
    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchDomainEvents()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new TestDbContext(options, dispatcher);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var aggregate = new TestAggregate(TestAggregateId.Create(Guid.NewGuid()));
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        await context.SaveChangesAsync();

        await dispatcher.Received(1)
            .DispatchAsync(
                Arg.Any<IEnumerable<IDomainEvent>>(),
                Arg.Any<CancellationToken>());
    }
}

#region Test Infrastructure

public sealed class TestDbContext : HectorDbContext
{
    public TestDbContext(
        DbContextOptions<TestDbContext> options,
        IDomainEventDispatcher dispatcher)
        : base(options, dispatcher)
    {
    }

    public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var converter = new ValueConverter<TestAggregateId, Guid>(
            id => id.Value,
            value => TestAggregateId.Create(value));

        modelBuilder.Entity<TestAggregate>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                   .HasConversion(converter)
                   .ValueGeneratedNever();
        });
    }
}

public sealed class TestAggregate : AggregateRoot<TestAggregateId>
{
    public TestAggregate(TestAggregateId id)
        : base(id)
    {
    }

    private TestAggregate() : base(null!) { }

    public void RaiseTestEvent()
    {
        RaiseDomainEvent(new TestDomainEvent(Id));
    }
}

public sealed record TestDomainEvent(TestAggregateId AggregateId)
    : DomainEventBase;

public sealed class TestAggregateId
    : StronglyTypedIdCrtp<TestAggregateId>,
      IStronglyTypedId<TestAggregateId>
{
    private TestAggregateId(Guid value)
        : base(value)
    {
    }

    private TestAggregateId()
        : base(Guid.Empty)
    {
    }

    public static TestAggregateId Create(Guid value)
        => new(value);

    public static TestAggregateId CreateEmpty()
        => new(Guid.Empty);
}

#endregion
