using Hector.BuildingBlocks.Domain.Primitives;
using Hector.BuildingBlocks.Persistence.Converters;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Hector.BuildingBlocks.Persistence.IntegrationTests;

public sealed class TestOrderId : StronglyTypedId<TestOrderId>
{
    private TestOrderId(Guid value) : base(value) { }

    public static TestOrderId New()
        => CreateNew(v => new TestOrderId(v));

    internal static TestOrderId From(Guid value)
        => FromExisting(value, v => new TestOrderId(v));
}

public class TestOrder
{
    public TestOrderId Id { get; set; } = null!;
    public string OrderNumber { get; set; } = null!;
}

#region DbContexts

public class TestDbContextWithoutConvention : DbContext
{
    public DbSet<TestOrder> Orders => Set<TestOrder>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        optionsBuilder.UseSqlite(connection);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestOrder>().HasKey(x => x.Id);
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
        // 1. پیدا کردن همه کلاس‌هایی که از StronglyTypedId<> ارث می‌برند در اسمبلی تست
        var stronglyTypedIdTypes = typeof(TestOrderId).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface &&
                        t.BaseType != null &&
                        t.BaseType.IsGenericType &&
                        t.BaseType.GetGenericTypeDefinition() == typeof(StronglyTypedId<>));

        // 2. ثبت converter برای هر کدام به صورت جداگانه
        foreach (var type in stronglyTypedIdTypes)
        {
            var converterType = typeof(StronglyTypedIdValueConverter<>).MakeGenericType(type);
            configurationBuilder.Properties(type).HaveConversion(converterType);
        }
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
    public void EfCore_Should_Fail_To_Map_StronglyTypedId_Without_Convention()
    {
        using var context = new TestDbContextWithoutConvention();

        var action = () => context.Model;

        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void EfCore_Should_Map_StronglyTypedId_With_Convention()
    {
        using var context = new TestDbContextWithConvention();

        var exception = Record.Exception(() => context.Model);

        Assert.Null(exception);
    }
}
