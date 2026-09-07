using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;
using OrderManager.Web.Services;

namespace OrderManager.Web.Tests;

public class CustomerServiceTests
{
    private sealed class TestFactory(Func<AppDbContext> factory) : IDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext() => factory();
    }

    private static AppDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new AppDbContext(options);
    }

    private static CustomerService BuildService(string dbName) =>
        new(new TestFactory(() => CreateDb(dbName)));

    [Fact]
    public async Task SoftDeleteAsync_SetsDeletedAtToUtcNow()
    {
        var dbName = nameof(SoftDeleteAsync_SetsDeletedAtToUtcNow);
        await using (var db = CreateDb(dbName))
        {
            db.Customers.Add(new Customer { Name = "Ana" });
            await db.SaveChangesAsync();
        }

        var before = DateTime.UtcNow.AddSeconds(-1);
        await using (var db2 = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.SoftDeleteAsync(1);
        }
        var after = DateTime.UtcNow.AddSeconds(1);

        await using var db3 = CreateDb(dbName);
        var customer = await db3.Customers.AsNoTracking().FirstAsync(c => c.Id == 1);
        Assert.NotNull(customer.DeletedAt);
        Assert.InRange(customer.DeletedAt!.Value, before, after);
    }

    [Fact]
    public async Task SoftDeleteAsync_UnknownId_IsNoOp()
    {
        var dbName = nameof(SoftDeleteAsync_UnknownId_IsNoOp);
        await using (var db = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.SoftDeleteAsync(42);
        }
        await using var db2 = CreateDb(dbName);
        Assert.Empty(await db2.Customers.ToListAsync());
    }

    [Fact]
    public async Task GetActiveAsync_ExcludesSoftDeleted()
    {
        var dbName = nameof(GetActiveAsync_ExcludesSoftDeleted);
        await using (var db = CreateDb(dbName))
        {
            db.Customers.AddRange(
                new Customer { Name = "Ana" },
                new Customer { Name = "Beto" },
                new Customer { Name = "Carla", DeletedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        await using var db2 = CreateDb(dbName);
        var service = BuildService(dbName);
        var active = await service.GetActiveAsync();

        var names = active.Select(c => c.Name).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "Ana", "Beto" }, names);
    }

    [Fact]
    public async Task RestoreAsync_NullsDeletedAt()
    {
        var dbName = nameof(RestoreAsync_NullsDeletedAt);
        await using (var db = CreateDb(dbName))
        {
            db.Customers.Add(new Customer { Name = "Ana", DeletedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        await using (var db2 = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.RestoreAsync(1);
        }

        await using var db3 = CreateDb(dbName);
        var customer = await db3.Customers.AsNoTracking().FirstAsync(c => c.Id == 1);
        Assert.Null(customer.DeletedAt);
    }

    [Fact]
    public async Task GetDeletedAsync_ReturnsOnlySoftDeleted()
    {
        var dbName = nameof(GetDeletedAsync_ReturnsOnlySoftDeleted);
        await using (var db = CreateDb(dbName))
        {
            db.Customers.AddRange(
                new Customer { Name = "Ana" },
                new Customer { Name = "Beto", DeletedAt = DateTime.UtcNow },
                new Customer { Name = "Carla", DeletedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        await using var db2 = CreateDb(dbName);
        var service = BuildService(dbName);
        var deleted = await service.GetDeletedAsync();

        var names = deleted.Select(c => c.Name).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "Beto", "Carla" }, names);
    }

    [Fact]
    public async Task CountOrdersAsync_ReturnsOrderCountForCustomer()
    {
        var dbName = nameof(CountOrdersAsync_ReturnsOrderCountForCustomer);
        await using (var db = CreateDb(dbName))
        {
            var ana = new Customer { Name = "Ana" };
            var beto = new Customer { Name = "Beto" };
            db.AddRange(ana, beto);
            db.Orders.Add(new Order
            {
                CustomerId = ana.Id,
                DeliveryAt = DateTime.Now.AddDays(1),
                Lines = { new OrderLine { ProductId = 1, Quantity = 1, UnitPrice = 1m } },
            });
            db.Orders.Add(new Order
            {
                CustomerId = ana.Id,
                DeliveryAt = DateTime.Now.AddDays(2),
                Lines = { new OrderLine { ProductId = 1, Quantity = 1, UnitPrice = 1m } },
            });
            await db.SaveChangesAsync();
        }

        await using var db2 = CreateDb(dbName);
        var service = BuildService(dbName);
        Assert.Equal(2, await service.CountOrdersAsync(1));
        Assert.Equal(0, await service.CountOrdersAsync(2));
    }

    [Fact]
    public async Task OrdersQuery_StillResolvesSoftDeletedCustomerName()
    {
        var dbName = nameof(OrdersQuery_StillResolvesSoftDeletedCustomerName);
        await using (var db = CreateDb(dbName))
        {
            var ana = new Customer { Name = "Ana" };
            db.Add(ana);
            db.Orders.Add(new Order
            {
                CustomerId = ana.Id,
                DeliveryAt = DateTime.Now.AddDays(1),
                Lines = { new OrderLine { ProductId = 1, Quantity = 1, UnitPrice = 1m } },
            });
            await db.SaveChangesAsync();
        }

        await using (var db2 = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.SoftDeleteAsync(1);
        }

        await using var db3 = CreateDb(dbName);
        var name = await db3.Orders
            .Include(o => o.Customer)
            .Select(o => o.Customer!.Name)
            .FirstAsync();
        Assert.Equal("Ana", name);
    }
}
