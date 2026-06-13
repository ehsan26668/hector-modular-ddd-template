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
    public async Task Should_StoreMessage_When_FirstTimeProcessing()
    {
        // Arrange
        using var connection = PersistenceTestInfrastructure.CreateOpenSqliteConnection();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        var assemblyProvider = Substitute.For<IStronglyTypedIdAssemblyProvider>();
        var outboxSerializer = new SystemTextJsonOutboxEventSerializer(new CachedOutboxEventTypeResolver());

        await using var context = new TestDbContext(options, assemblyProvider, outboxSerializer);
        await context.Database.EnsureCreatedAsync();

        var store = new EfCoreInboxStore(context);

        var messageId = Guid.NewGuid();
        var consumer = "TestConsumer";

        // Act
        var stored = await store.TryStoreAsync(messageId, consumer);

        // Assert
        stored.Should().BeTrue();

        var count = await context.InboxMessages
            .CountAsync(x => x.MessageId == messageId && x.Consumer == consumer);

        count.Should().Be(1);
    }

    [Fact]
    public async Task Should_ReturnFalse_When_MessageAlreadyStored()
    {
        // Arrange
        using var connection = PersistenceTestInfrastructure.CreateOpenSqliteConnection();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        var assemblyProvider = Substitute.For<IStronglyTypedIdAssemblyProvider>();
        var outboxSerializer = new SystemTextJsonOutboxEventSerializer(new CachedOutboxEventTypeResolver());

        await using var context = new TestDbContext(options, assemblyProvider, outboxSerializer);
        await context.Database.EnsureCreatedAsync();

        var store = new EfCoreInboxStore(context);

        var messageId = Guid.NewGuid();
        var consumer = "TestConsumer";

        await store.TryStoreAsync(messageId, consumer);

        // Act
        var storedAgain = await store.TryStoreAsync(messageId, consumer);

        // Assert
        storedAgain.Should().BeFalse();

        var count = await context.InboxMessages
            .CountAsync(x => x.MessageId == messageId && x.Consumer == consumer);

        count.Should().Be(1);
    }

    [Fact]
    public async Task Should_AllowSameMessage_ForDifferentConsumers()
    {
        // Arrange
        using var connection = PersistenceTestInfrastructure.CreateOpenSqliteConnection();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        var assemblyProvider = Substitute.For<IStronglyTypedIdAssemblyProvider>();
        var outboxSerializer = new SystemTextJsonOutboxEventSerializer(new CachedOutboxEventTypeResolver());

        await using var context = new TestDbContext(options, assemblyProvider, outboxSerializer);
        await context.Database.EnsureCreatedAsync();

        var store = new EfCoreInboxStore(context);

        var messageId = Guid.NewGuid();

        // Act
        var firstConsumerStored = await store.TryStoreAsync(messageId, "ConsumerA");
        var secondConsumerStored = await store.TryStoreAsync(messageId, "ConsumerB");

        // Assert
        firstConsumerStored.Should().BeTrue();
        secondConsumerStored.Should().BeTrue();

        var count = await context.InboxMessages
            .CountAsync(x => x.MessageId == messageId);

        count.Should().Be(2);
    }

    private sealed class TestDbContext(
        DbContextOptions options,
        IStronglyTypedIdAssemblyProvider assemblyProvider,
        IOutboxEventSerializer outboxSerializer)
        : HectorDbContext(options, assemblyProvider, outboxSerializer)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboxMessage).Assembly);
        }
    }
}
