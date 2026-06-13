using FluentAssertions;
using Hector.BuildingBlocks.Persistence.Inbox;
using Hector.BuildingBlocks.Persistence.Outbox;
using Hector.Testing.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using static Hector.Testing.Persistence.PersistenceTestInfrastructure;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class InboxPersistenceTests
{
    [Fact]
    public async Task Should_EnforceUniqueConstraint_OnMessageIdAndConsumer()
    {
        // Arrange
        using var connection = PersistenceTestInfrastructure.CreateOpenSqliteConnection();
        var options = new DbContextOptionsBuilder<TestDbContext>().UseSqlite(connection).Options;

        var assemblyProvider = Substitute.For<IStronglyTypedIdAssemblyProvider>();
        var outboxSerializer = new SystemTextJsonOutboxEventSerializer(
            new AttributedOutboxEventTypeResolver(
                [typeof(TestDomainEvent).Assembly]));

        await using var context = new TestDbContext(options, assemblyProvider, outboxSerializer);
        await context.Database.EnsureCreatedAsync();

        var messageId = Guid.NewGuid();
        var consumer = "TestConsumer";

        context.InboxMessages.Add(new InboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            Consumer = consumer,
            ProcessedOn = DateTime.UtcNow
        });

        context.InboxMessages.Add(new InboxMessage
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            Consumer = consumer,
            ProcessedOn = DateTime.UtcNow
        });

        // Act
        var act = async () => await context.SaveChangesAsync();

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
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
