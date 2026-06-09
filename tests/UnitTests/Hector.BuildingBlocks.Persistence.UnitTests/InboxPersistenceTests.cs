using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class InboxPersistenceTests
{
    [Fact]
    public async Task Should_PersistInboxMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var assemblyProvider = Substitute.For<IStronglyTypedIdAssemblyProvider>();
        var outboxSerializer = new SystemTextJsonOutboxEventSerializer(
            new CachedOutboxEventTypeResolver());

        await using var context = new TestDbContext(options, assemblyProvider, outboxSerializer);

        var message = new Inbox.InboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = Guid.NewGuid(),
            Consumer = "TestConsumer",
            ProcessedOn = DateTime.UtcNow
        };

        context.InboxMessages.Add(message);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var messages = await context.InboxMessages.ToListAsync();

        messages.Should().HaveCount(1);
        messages[0].MessageId.Should().Be(message.MessageId);
        messages[0].Consumer.Should().Be("TestConsumer");
    }

    private sealed class TestDbContext : HectorDbContext
    {
        public TestDbContext(
            DbContextOptions options,
            IStronglyTypedIdAssemblyProvider assemblyProvider,
            IOutboxEventSerializer outboxSerializer)
            : base(options, assemblyProvider, outboxSerializer) { }

        public DbSet<Inbox.InboxMessage> InboxMessages => Set<Inbox.InboxMessage>();
    }
}