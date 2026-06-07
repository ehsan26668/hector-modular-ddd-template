using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public class HectorDbContextTests
{
    private static readonly IStronglyTypedIdAssemblyProvider StronglyTypedIdAssemblyProvider =
        new TestStronglyTypedIdAssemblyProvider();

    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchDomainEvents()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new TestDbContext(
            options,
            dispatcher,
            StronglyTypedIdAssemblyProvider);

        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var aggregate = new TestAggregate(TestAggregateId.New());
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

        await using var context = new TestDbContext(
            options,
            dispatcher,
            StronglyTypedIdAssemblyProvider);

        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var aggregate = new TestAggregate(TestAggregateId.New());
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

        await using var context = new FailingDbContext(
            options,
            dispatcher,
            StronglyTypedIdAssemblyProvider);

        var aggregate = new TestAggregate(TestAggregateId.New());
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync());

        await dispatcher.DidNotReceive()
            .DispatchAsync(
                Arg.Any<IEnumerable<IDomainEvent>>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotClearDomainEvents_WhenPersistenceFails()
    {
        var dispatcher = Substitute.For<IDomainEventDispatcher>();

        var options = new DbContextOptionsBuilder<FailingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new FailingDbContext(
            options,
            dispatcher,
            StronglyTypedIdAssemblyProvider);

        var aggregate = new TestAggregate(TestAggregateId.New());
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
            .DispatchAsync(
                Arg.Any<IEnumerable<IDomainEvent>>(),
                Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Dispatch failed"));

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new TestDbContext(
            options,
            dispatcher,
            StronglyTypedIdAssemblyProvider);

        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var aggregate = new TestAggregate(TestAggregateId.New());
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
        IDomainEventDispatcher dispatcher,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider)
        : base(options, dispatcher, stronglyTypedIdAssemblyProvider)
    {
    }

    public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestAggregate>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
        });
    }
}

public sealed class FailingDbContext : HectorDbContext
{
    public FailingDbContext(
        DbContextOptions<FailingDbContext> options,
        IDomainEventDispatcher dispatcher,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider)
        : base(options, dispatcher, stronglyTypedIdAssemblyProvider)
    {
    }

    public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestAggregate>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
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

public sealed class TestAggregateId : StronglyTypedId<TestAggregateId>
{
    private TestAggregateId(Guid value) : base(value)
    {
    }

    public static TestAggregateId New()
        => CreateNew(v => new TestAggregateId(v));

    internal static TestAggregateId From(Guid value)
        => FromExisting(value, v => new TestAggregateId(v));
}

public sealed class TestStronglyTypedIdAssemblyProvider
    : IStronglyTypedIdAssemblyProvider
{
    public IReadOnlyCollection<Assembly> GetAssemblies()
    {
        return new[]
        {
            typeof(TestAggregateId).Assembly
        };
    }
}

#endregion
