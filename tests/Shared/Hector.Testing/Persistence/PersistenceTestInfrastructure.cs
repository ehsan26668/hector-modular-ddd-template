using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Hector.Testing.Persistence;

public static class PersistenceTestInfrastructure
{
    public static IStronglyTypedIdAssemblyProvider StronglyTypedIdAssemblyProvider { get; } =
        new TestStronglyTypedIdAssemblyProvider();

    public static IOutboxEventSerializer OutboxSerializer { get; } =
        new SystemTextJsonOutboxEventSerializer(new CachedOutboxEventTypeResolver());

    public static SqliteConnection CreateOpenInMemoryConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    public static async Task<TestDbContext> CreateContextAsync(
        SqliteConnection connection,
        IDomainEventDispatcher? domainEventDispatcher = null)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestDbContext(
            options,
            StronglyTypedIdAssemblyProvider,
            OutboxSerializer,
            domainEventDispatcher ?? new RecordingDomainEventDispatcher());

        await context.Database.EnsureCreatedAsync();
        return context;
    }

    public static async Task<FailingDbContext> CreateFailingContextAsync(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<FailingDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new FailingDbContext(
            options,
            StronglyTypedIdAssemblyProvider,
            OutboxSerializer,
            new RecordingDomainEventDispatcher());

        await context.Database.EnsureCreatedAsync();
        return context;
    }

    public sealed class TestStronglyTypedIdAssemblyProvider : IStronglyTypedIdAssemblyProvider
    {
        public IReadOnlyCollection<Assembly> GetAssemblies()
            => [typeof(TestAggregateId).Assembly];
    }

    public sealed class RecordingDomainEventDispatcher : IDomainEventDispatcher
    {
        public List<IDomainEvent> DispatchedEvents { get; } = [];

        public Task DispatchAsync(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default)
        {
            DispatchedEvents.AddRange(domainEvents);
            return Task.CompletedTask;
        }
    }

    public class TestDbContext(
        DbContextOptions<TestDbContext> options,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider,
        IOutboxEventSerializer outboxSerializer,
        IDomainEventDispatcher domainEventDispatcher)
        : HectorDbContext(options, stronglyTypedIdAssemblyProvider, outboxSerializer, domainEventDispatcher)
    {
        public DbSet<TestAggregate> TestAggregates => Set<TestAggregate>();

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

    public sealed class FailingDbContext(
        DbContextOptions<FailingDbContext> options,
        IStronglyTypedIdAssemblyProvider stronglyTypedIdAssemblyProvider,
        IOutboxEventSerializer outboxSerializer,
        IDomainEventDispatcher domainEventDispatcher)
        : HectorDbContext(options, stronglyTypedIdAssemblyProvider, outboxSerializer, domainEventDispatcher)
    {
        public DbSet<TestAggregate> TestAggregates => Set<TestAggregate>();

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
            => throw new DbUpdateException("Simulated persistence failure.", innerException: null);
    }

    public sealed class TestAggregate : AggregateRoot<TestAggregateId>
    {
        private TestAggregate(TestAggregateId id) : base(id) { }

        public static TestAggregate Create() => new(TestAggregateId.New());

        public void RaiseTestEvent() => RaiseDomainEvent(new TestDomainEvent(Id.Value));
    }

    public sealed class TestAggregateId(Guid value) : StronglyTypedId<TestAggregateId>(value)
    {
        public static TestAggregateId New() => CreateNew(id => new TestAggregateId(id));

        public static TestAggregateId From(Guid value) => FromExisting(value, id => new TestAggregateId(id));
    }

    public sealed record TestDomainEvent(Guid AggregateId) : DomainEventBase;
}
