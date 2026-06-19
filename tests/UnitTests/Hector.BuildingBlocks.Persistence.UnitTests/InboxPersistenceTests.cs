using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Inbox;
using Hector.Persistence.Testing;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

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

        await using var context = new TestDbContext(
            options,
            assemblyProvider,
            new NoOpDomainEventDispatcher());

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
        IDomainEventDispatcher domainEventDispatcher)
        : HectorDbContext(options, assemblyProvider, domainEventDispatcher)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboxMessage).Assembly);
        }
    }

    private sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(
            IEnumerable<IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
