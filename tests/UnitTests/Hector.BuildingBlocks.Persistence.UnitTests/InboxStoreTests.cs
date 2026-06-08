using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class InboxStoreTests
{
    [Fact]
    public async Task Should_DetectDuplicateMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var assemblyProvider = Substitute.For<IStronglyTypedIdAssemblyProvider>();

        await using var context = new TestDbContext(options, assemblyProvider);

        var store = new Inbox.EfCoreInboxStore(context);

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
        public TestDbContext(
            DbContextOptions options,
            IStronglyTypedIdAssemblyProvider assemblyProvider)
            : base(options, assemblyProvider) { }

        public DbSet<Inbox.InboxMessage> InboxMessages => Set<Inbox.InboxMessage>();
    }
}