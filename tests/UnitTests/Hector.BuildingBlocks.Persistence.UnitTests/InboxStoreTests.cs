using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Inbox;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Testing.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class InboxStoreTests
{
    [Fact]
    public async Task Should_DetectDuplicateMessage()
    {
        // Arrange
        using var connection = PersistenceTestInfrastructure.CreateOpenSqliteConnection();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;

        var assemblyProvider = Substitute.For<IStronglyTypedIdAssemblyProvider>();
        var outboxSerializer = new SystemTextJsonOutboxEventSerializer(new CachedOutboxEventTypeResolver());

        await using var context = new TestDbContext(options, assemblyProvider, outboxSerializer);
        await context.Database.EnsureCreatedAsync();

        var store = new EfCoreInboxStore(context);

        var messageId = Guid.NewGuid();
        var consumer = "TestConsumer";

        await store.StoreAsync(messageId, consumer);

        // Act
        var exists = await store.ExistsAsync(messageId, consumer);

        // Assert
        exists.Should().BeTrue();
    }

    private sealed class TestDbContext : HectorDbContext
    {
        public TestDbContext(DbContextOptions options, IStronglyTypedIdAssemblyProvider assemblyProvider, IOutboxEventSerializer outboxSerializer)
            : base(options, assemblyProvider, outboxSerializer) { }

        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboxMessage).Assembly);
        }
    }
}
