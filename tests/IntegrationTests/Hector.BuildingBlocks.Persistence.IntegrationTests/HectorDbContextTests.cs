using System.Reflection;
using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public class HectorDbContextTests
{
    private static readonly IStronglyTypedIdAssemblyProvider StronglyTypedIdAssemblyProvider =
        new TestStronglyTypedIdAssemblyProvider();

    private static readonly IOutboxEventSerializer outboxSerializer =
        new SystemTextJsonOutboxEventSerializer(new CachedOutboxEventTypeResolver());

    private static async Task<TestDbContext> CreateContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestDbContext(
            options,
            StronglyTypedIdAssemblyProvider,
            outboxSerializer);

        await context.Database.EnsureCreatedAsync();

        return context;
    }

    private static async Task<FailingDbContext> CreateFailingContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<FailingDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new FailingDbContext(
            options,
            StronglyTypedIdAssemblyProvider,
            outboxSerializer);

        await context.Database.EnsureCreatedAsync();

        return context;
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistOutboxMessage_WhenAggregateRaisesDomainEvent()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = await CreateContextAsync(connection);

        var aggregate = new TestAggregate(TestAggregateId.New());
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        await context.SaveChangesAsync();

        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();

        outboxMessages.Should().ContainSingle();

        outboxMessages[0].Type.Should()
            .Be(typeof(TestDomainEvent).AssemblyQualifiedName);

        outboxMessages[0].Content.Should()
            .Contain(aggregate.Id.Value.ToString());
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldClearDomainEvents_AfterSuccessfulPersistence()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = await CreateContextAsync(connection);

        var aggregate = new TestAggregate(TestAggregateId.New());
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        await context.SaveChangesAsync();

        ((IHasDomainEvents)aggregate)
            .GetDomainEvents()
            .Should()
            .BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotClearDomainEvents_WhenPersistenceFails()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = await CreateFailingContextAsync(connection);

        var aggregate = new TestAggregate(TestAggregateId.New());
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync());

        ((IHasDomainEvents)aggregate)
            .GetDomainEvents()
            .Should()
            .NotBeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldNotPersistOutboxMessage_WhenPersistenceFails()
    {
        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using var context = await CreateFailingContextAsync(connection);

        var aggregate = new TestAggregate(TestAggregateId.New());
        aggregate.RaiseTestEvent();

        context.Add(aggregate);

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            context.SaveChangesAsync());

        var outboxMessages = await context.Set<OutboxMessage>().ToListAsync();

        outboxMessages.Should().BeEmpty();
    }
}

#region Test Infrastructure

public sealed class TestDbContext : HectorDbContext
{
    public TestDbContext(
        DbContextOptions<TestDbContext> options,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider,
        IOutboxEventSerializer outboxSerializer)
        : base(options, stronglyTypedIdAssemblyProvider, outboxSerializer)
    {
    }

    public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestAggregate>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();
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
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider,
        IOutboxEventSerializer outboxSerializer)
        : base(options, stronglyTypedIdAssemblyProvider, outboxSerializer)
    {
    }

    public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TestAggregate>(builder =>
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedNever();
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
