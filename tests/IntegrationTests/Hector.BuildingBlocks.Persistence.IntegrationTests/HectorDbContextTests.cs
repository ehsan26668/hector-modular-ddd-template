using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public class HectorDbContextTests
{
    private static readonly IStronglyTypedIdAssemblyProvider StronglyTypedIdAssemblyProvider =
        new TestStronglyTypedIdAssemblyProvider();

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistOutboxMessage_WhenAggregateRaisesDomainEvent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new TestDbContext(options, StronglyTypedIdAssemblyProvider);

        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var aggregate = new TestAggregate(TestAggregateId.New());
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();

        outboxMessages.Should().ContainSingle();
        outboxMessages[0].Type.Should().Be(typeof(TestDomainEvent).FullName);
        outboxMessages[0].Content.Should().Contain(aggregate.Id.Value.ToString());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldClearDomainEvents_AfterSuccessfulPersistence()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new TestDbContext(options, StronglyTypedIdAssemblyProvider);

        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var aggregate = new TestAggregate(TestAggregateId.New());
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        ((IHasDomainEvents)aggregate).GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotClearDomainEvents_WhenPersistenceFails()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FailingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new FailingDbContext(options, StronglyTypedIdAssemblyProvider);

        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var aggregate = new TestAggregate(TestAggregateId.New());
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        // Assert
        ((IHasDomainEvents)aggregate).GetDomainEvents().Should().NotBeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotPersistOutboxMessage_WhenPersistenceFails()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<FailingDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        await using var context = new FailingDbContext(options, StronglyTypedIdAssemblyProvider);

        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        var aggregate = new TestAggregate(TestAggregateId.New());
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        // Act
        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());

        // Assert
        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();
        outboxMessages.Should().BeEmpty();
    }
}

#region Test Infrastructure

public sealed class TestDbContext : HectorDbContext
{
    public TestDbContext(
        DbContextOptions<TestDbContext> options,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider)
        : base(options, stronglyTypedIdAssemblyProvider)
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

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(x => x.Id);
        });
    }
}

public sealed class FailingDbContext : HectorDbContext
{
    public FailingDbContext(
        DbContextOptions<FailingDbContext> options,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider)
        : base(options, stronglyTypedIdAssemblyProvider)
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

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(x => x.Id);
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

public sealed record TestDomainEvent(TestAggregateId AggregateId) : DomainEventBase;

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

public sealed class TestStronglyTypedIdAssemblyProvider : IStronglyTypedIdAssemblyProvider
{
    public IReadOnlyCollection<Assembly> GetAssemblies()
    {
        return new[] { typeof(TestAggregateId).Assembly };
    }
}

#endregion
