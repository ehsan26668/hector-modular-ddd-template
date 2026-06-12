using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Inbox;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Testing.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class InboxPersistenceTests
{
    [Fact]
    public async Task Should_PersistInboxMessage()
    {
        // Arrange
        using var connection = PersistenceTestInfrastructure.CreateOpenSqliteConnection();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;

        var assemblyProvider = Substitute.For<IStronglyTypedIdAssemblyProvider>();
        var outboxSerializer = new SystemTextJsonOutboxEventSerializer(new CachedOutboxEventTypeResolver());

        await using var context = new TestDbContext(options, assemblyProvider, outboxSerializer);
        await context.Database.EnsureCreatedAsync();

        var message = new InboxMessage
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
