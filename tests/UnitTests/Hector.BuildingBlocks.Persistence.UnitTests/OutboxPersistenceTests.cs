using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Hector.BuildingBlocks.Persistence.UnitTests;

public sealed class OutboxPersistenceTests
{
    [Fact]
    public async Task Should_PersistOutboxMessage_When_AggregateContainsDomainEvent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var assembblyProvider = Substitute.For<IStronglyTypedIdAssemblyProvider>();
        var outboxSerializer = new SystemTextJsonOutboxEventSerializer(
            new CachedOutboxEventTypeResolver());
        var domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();

        await using var context = new TestDbContext(
            options,
            assembblyProvider,
            outboxSerializer,
            domainEventDispatcher);

        var aggregate = TestAggregate.Create();
        context.Aggregates.Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        var messages = await context.OutboxMessages.ToListAsync();

        messages.Should().HaveCount(1);
        messages[0].Type.Should().Be(typeof(TestDomainEvent).AssemblyQualifiedName);
        messages[0].ProcessedOn.Should().BeNull();
    }

    private sealed class TestDbContext : HectorDbContext
    {
        public TestDbContext(
            DbContextOptions options,
            IStronglyTypedIdAssemblyProvider assemblyProvider,
            IOutboxEventSerializer outboxSerializer,
            IDomainEventDispatcher domainEventDispatcher)
            : base(
                  options,
                  assemblyProvider,
                  outboxSerializer,
                  domainEventDispatcher)
        { }

        public DbSet<TestAggregate> Aggregates => Set<TestAggregate>();
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        private TestAggregate(Guid id) : base(id) { }

        public static TestAggregate Create()
        {
            var aggregate = new TestAggregate(Guid.NewGuid());
            aggregate.RaiseDomainEvent(new TestDomainEvent());
            return aggregate;
        }
    }

    private sealed record TestDomainEvent : DomainEventBase;
}