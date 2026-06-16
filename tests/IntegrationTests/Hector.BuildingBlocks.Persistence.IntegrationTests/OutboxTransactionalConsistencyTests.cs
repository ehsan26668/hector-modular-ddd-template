using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class OutboxTransactionalConsistencyTests
{
    [Fact]
    public async Task Should_NotPersistOutboxMessage_When_SaveOperationFails()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AtomicityTestDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setupContext = new AtomicityTestDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();
        }

        await using var failingContext = new AtomicityTestDbContext(options);

        failingContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "valid.type",
            Content = "{}",
            OccurredOn = DateTime.UtcNow,
            Version = 1
        });

        failingContext.FailingWriteEntities.Add(new FailingWriteEntity
        {
            Name = null!
        });

        // Act
        var act = async () => await failingContext.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();

        await using var verificationContext = new AtomicityTestDbContext(options);

        var persistedOutboxCount = await verificationContext.OutboxMessages.CountAsync();
        var persistedEntitiesCount = await verificationContext.FailingWriteEntities.CountAsync();

        persistedOutboxCount.Should().Be(0);
        persistedEntitiesCount.Should().Be(0);
    }

    private sealed class AtomicityTestDbContext(
    DbContextOptions<AtomicityTestDbContext> options)
    : HectorDbContext(
        options,
        new EmptyStronglyTypedIdAssemblyProvider(),
        new NoOpDomainEventDispatcher())
    {
        public DbSet<FailingWriteEntity> FailingWriteEntities => Set<FailingWriteEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FailingWriteEntity>(builder =>
            {
                builder.HasKey(entity => entity.Id);
                builder.Property(entity => entity.Name).IsRequired();
            });
        }
    }

    private sealed class FailingWriteEntity
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }

    private sealed class EmptyStronglyTypedIdAssemblyProvider : IStronglyTypedIdAssemblyProvider
    {
        public IReadOnlyCollection<System.Reflection.Assembly> GetAssemblies()
            => [];
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
