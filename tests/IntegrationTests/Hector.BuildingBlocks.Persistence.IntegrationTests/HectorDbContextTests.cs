using FluentAssertions;
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

    [Fact]
    public async Task SaveChangesAsync_ShouldClearDomainEvents_AfterDispatch()
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

        aggregate.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotDispatchEvents_WhenPersistenceFails()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        var options = new DbContextOptionsBuilder<FailingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new FailingDbContext(options, dispatcher);

        var aggregate = new TestAggregate(TestAggregateId.Create(Guid.NewGuid()));
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync());

        await dispatcher.DidNotReceive()
            .DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotClearDomainEvents_WhenPersistenceFails()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        var options = new DbContextOptionsBuilder<FailingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new FailingDbContext(options, dispatcher);

        var aggregate = new TestAggregate(TestAggregateId.Create(Guid.NewGuid()));
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync());

        aggregate.GetDomainEvents().Should().NotBeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPropagateException_WhenDispatchFails()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        dispatcher
            .DispatchAsync(Arg.Any<IEnumerable<IDomainEvent>>(), Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Dispatch failed"));

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new TestDbContext(options, dispatcher);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var aggregate = new TestAggregate(TestAggregateId.Create(Guid.NewGuid()));
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.SaveChangesAsync());
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

public sealed class FailingDbContext : HectorDbContext
{
    public FailingDbContext(
        DbContextOptions options,
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

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new DbUpdateException("Simulated failure");
    }
}

public sealed class TestAggregate : AggregateRoot<TestAggregateId>
{
    public TestAggregate(TestAggregateId id)
        : base(id)
    {
    }

#pragma warning disable CS8618
    private TestAggregate() : base(null!) { }
#pragma warning restore CS8618

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
