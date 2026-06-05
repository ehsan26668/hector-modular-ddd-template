using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class TestOrderId : StronglyTypedIdCrtp<TestOrderId>, IStronglyTypedId<TestOrderId>
{
    private TestOrderId(Guid value) : base(value) { }

    private TestOrderId() : base(Guid.Empty) { }

    public static TestOrderId Create(Guid value) => new(value);
    public static TestOrderId CreateEmpty() => new(Guid.Empty);
}

public class TestOrder
{
    public TestOrderId Id { get; set; } = null!;
    public string OrderNumber { get; set; } = null!;
}

#region DbContext

public class TestDbContextWithoutConvention : DbContext
{
    public DbSet<TestOrder> Orders => Set<TestOrder>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        optionsBuilder.UseSqlite(connection);
    }
}

public class TestDbContextWithConvention : DbContext
{
    public DbSet<TestOrder> Orders => Set<TestOrder>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        optionsBuilder.UseSqlite(connection);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.RegisterStronglyTypedIdConventions(typeof(TestOrderId).Assembly);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestOrder>().HasKey(x => x.Id);
    }
}

#endregion

public sealed class StronglyTypedIdMappingTests
{
    [Fact]
    public void EfCore_Should_Fail_To_Map_StronglyTypedId_Without_Converter()
    {
        // Arrange
        using var context = new TestDbContextWithoutConvention();

        // Act
        var action = () => context.Model;

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void EfCore_Should_Map_StronglyTypedId_With_Converter()
    {
        // Arrange
        using var context = new TestDbContextWithConvention();

        // Act
        var exception = Record.Exception(() => context.Model);

        // Assert
        Assert.Null(exception);
    }
}