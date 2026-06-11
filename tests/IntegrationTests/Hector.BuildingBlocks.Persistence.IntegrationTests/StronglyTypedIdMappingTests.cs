using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Converters;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using static Hector.Testing.Persistence.PersistenceTestInfrastructure;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class StronglyTypedIdMappingTests
{
    [Fact]
    public async Task Should_ThrowException_When_PersistingStronglyTypedIdWithoutConvention()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        await using var context = StronglyTypedIdTestDbContext.WithoutConvention(connection);

        var order = TestOrder.Create("ORD-001");

        // Act
        Func<Task> act = async () =>
        {
            await context.Database.EnsureCreatedAsync();
            context.Orders.Add(order);
            await context.SaveChangesAsync();
        };

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void Should_CreateModel_When_StronglyTypedIdConventionIsConfigured()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        using var context = StronglyTypedIdTestDbContext.WithConvention(connection);

        // Act
        var model = context.Model;

        // Assert
        model.Should().NotBeNull();
        model.FindEntityType(typeof(TestOrder)).Should().NotBeNull();
    }

    [Fact]
    public async Task Should_PersistAndRehydrate_StronglyTypedId_When_StronglyTypedIdConventionIsConfigured()
    {
        // Arrange
        using var connection = CreateOpenInMemoryConnection();
        var order = TestOrder.Create("ORD-001");

        await using (var setupContext = StronglyTypedIdTestDbContext.WithConvention(connection))
        {
            await setupContext.Database.EnsureCreatedAsync();
            setupContext.Orders.Add(order);
            await setupContext.SaveChangesAsync();
        }

        await using var assertionContext = StronglyTypedIdTestDbContext.WithConvention(connection);

        // Act
        var persistedOrder = await assertionContext.Orders.SingleAsync();

        // Assert
        persistedOrder.Id.Should().Be(order.Id);
        persistedOrder.OrderNumber.Should().Be(order.OrderNumber);
    }

    private sealed class StronglyTypedIdTestDbContext : DbContext
    {
        private readonly SqliteConnection _connection;
        private readonly bool _configureStronglyTypedIdConvention;

        private StronglyTypedIdTestDbContext(
            SqliteConnection connection,
            bool configureStronglyTypedIdConvention)
        {
            _connection = connection;
            _configureStronglyTypedIdConvention = configureStronglyTypedIdConvention;
        }

        public DbSet<TestOrder> Orders => Set<TestOrder>();

        public static StronglyTypedIdTestDbContext WithConvention(SqliteConnection connection)
            => new(connection, true);

        public static StronglyTypedIdTestDbContext WithoutConvention(SqliteConnection connection)
            => new(connection, false);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite(_connection);

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            if (!_configureStronglyTypedIdConvention)
            {
                return;
            }

            configurationBuilder
                .Properties<TestOrderId>()
                .HaveConversion<StronglyTypedIdValueConverter<TestOrderId>>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestOrder>(builder =>
            {
                builder.HasKey(o => o.Id);
                builder.Property(o => o.OrderNumber).IsRequired();
            });
        }
    }

    private sealed class TestOrder
    {
        private TestOrder(TestOrderId id, string orderNumber)
        {
            Id = id;
            OrderNumber = orderNumber;
        }

        private TestOrder() { }

        public TestOrderId Id { get; private set; } = null!;
        public string OrderNumber { get; private set; } = null!;

        public static TestOrder Create(string orderNumber)
            => new(TestOrderId.New(), orderNumber);
    }

    private sealed class TestOrderId : StronglyTypedId<TestOrderId>
    {
        private TestOrderId(Guid value) : base(value) { }

        public static TestOrderId New()
            => CreateNew(static value => new TestOrderId(value));
    }
}
