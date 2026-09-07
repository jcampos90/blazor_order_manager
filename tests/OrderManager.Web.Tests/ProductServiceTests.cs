using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;
using OrderManager.Web.Services;

namespace OrderManager.Web.Tests;

public class ProductServiceTests
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

    private static ProductService BuildService(string dbName) =>
        new(new TestFactory(() => CreateDb(dbName)));

    [Fact]
    public async Task SoftDeleteAsync_SetsDeletedAtToUtcNow()
    {
        var dbName = nameof(SoftDeleteAsync_SetsDeletedAtToUtcNow);
        await using (var db = CreateDb(dbName))
        {
            db.Products.Add(new Product { Name = "Pan", PrepHours = 24, Price = 5m });
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
        var product = await db3.Products.AsNoTracking().FirstAsync(p => p.Id == 1);
        Assert.NotNull(product.DeletedAt);
        Assert.InRange(product.DeletedAt!.Value, before, after);
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
        Assert.Empty(await db2.Products.ToListAsync());
    }

    [Fact]
    public async Task GetActiveAsync_ExcludesSoftDeleted()
    {
        var dbName = nameof(GetActiveAsync_ExcludesSoftDeleted);
        await using (var db = CreateDb(dbName))
        {
            db.Products.AddRange(
                new Product { Name = "Pan", PrepHours = 24, Price = 5m, IsActive = true },
                new Product { Name = "Facturas", PrepHours = 8, Price = 7m, IsActive = true },
                new Product { Name = "Torta", PrepHours = 48, Price = 25m, IsActive = false, DeletedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        await using var db2 = CreateDb(dbName);
        var service = BuildService(dbName);
        var active = await service.GetActiveAsync();

        var names = active.Select(p => p.Name).OrderBy(n => n).ToList();
        Assert.Equal(new[] { "Facturas", "Pan" }, names);
    }

    [Fact]
    public async Task GetOfferedAsync_FiltersInactiveAndSoftDeleted()
    {
        var dbName = nameof(GetOfferedAsync_FiltersInactiveAndSoftDeleted);
        await using (var db = CreateDb(dbName))
        {
            db.Products.AddRange(
                new Product { Name = "Pan", PrepHours = 24, Price = 5m, IsActive = true },
                new Product { Name = "Facturas", PrepHours = 8, Price = 7m, IsActive = false },
                new Product { Name = "Torta", PrepHours = 48, Price = 25m, IsActive = true, DeletedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        await using var db2 = CreateDb(dbName);
        var service = BuildService(dbName);
        var offered = await service.GetOfferedAsync();

        Assert.Equal(new[] { "Pan" }, offered.Select(p => p.Name).ToArray());
    }

    [Fact]
    public async Task RestoreAsync_NullsDeletedAt()
    {
        var dbName = nameof(RestoreAsync_NullsDeletedAt);
        await using (var db = CreateDb(dbName))
        {
            db.Products.Add(new Product { Name = "Pan", PrepHours = 24, Price = 5m, DeletedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        await using (var db2 = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.RestoreAsync(1);
        }

        await using var db3 = CreateDb(dbName);
        var product = await db3.Products.AsNoTracking().FirstAsync(p => p.Id == 1);
        Assert.Null(product.DeletedAt);
    }

    [Fact]
    public async Task GetDeletedAsync_ReturnsOnlySoftDeleted()
    {
        var dbName = nameof(GetDeletedAsync_ReturnsOnlySoftDeleted);
        await using (var db = CreateDb(dbName))
        {
            db.Products.AddRange(
                new Product { Name = "Pan", PrepHours = 24, Price = 5m },
                new Product { Name = "Facturas", PrepHours = 8, Price = 7m, DeletedAt = DateTime.UtcNow });
            await db.SaveChangesAsync();
        }

        await using var db2 = CreateDb(dbName);
        var service = BuildService(dbName);
        var deleted = await service.GetDeletedAsync();

        Assert.Equal(new[] { "Facturas" }, deleted.Select(p => p.Name).ToArray());
    }

    [Fact]
    public async Task ToggleActiveAsync_DoesNotChangeDeletedAt()
    {
        var dbName = nameof(ToggleActiveAsync_DoesNotChangeDeletedAt);
        await using (var db = CreateDb(dbName))
        {
            db.Products.Add(new Product { Name = "Pan", PrepHours = 24, Price = 5m, IsActive = true });
            await db.SaveChangesAsync();
        }

        await using (var db2 = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.ToggleActiveAsync(1);
        }

        await using var db3 = CreateDb(dbName);
        var product = await db3.Products.AsNoTracking().FirstAsync(p => p.Id == 1);
        Assert.False(product.IsActive);
        Assert.Null(product.DeletedAt);
    }

    [Fact]
    public async Task SoftDeleteAsync_DoesNotChangeIsActive()
    {
        var dbName = nameof(SoftDeleteAsync_DoesNotChangeIsActive);
        await using (var db = CreateDb(dbName))
        {
            db.Products.Add(new Product { Name = "Pan", PrepHours = 24, Price = 5m, IsActive = false });
            await db.SaveChangesAsync();
        }

        await using (var db2 = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.SoftDeleteAsync(1);
        }

        await using var db3 = CreateDb(dbName);
        var product = await db3.Products.AsNoTracking().FirstAsync(p => p.Id == 1);
        Assert.False(product.IsActive);
        Assert.NotNull(product.DeletedAt);
    }

    [Fact]
    public async Task CountUsageAsync_ReturnsLineAndOrderCounts()
    {
        var dbName = nameof(CountUsageAsync_ReturnsLineAndOrderCounts);
        await using (var db = CreateDb(dbName))
        {
            var ana = new Customer { Name = "Ana" };
            var pan = new Product { Name = "Pan", PrepHours = 24, Price = 5m };
            var facturas = new Product { Name = "Facturas", PrepHours = 8, Price = 7m };
            db.AddRange(ana, pan, facturas);
            db.Orders.Add(new Order
            {
                CustomerId = ana.Id,
                DeliveryAt = DateTime.Now.AddDays(1),
                Lines =
                {
                    new OrderLine { ProductId = pan.Id, Quantity = 2, UnitPrice = 5m },
                    new OrderLine { ProductId = facturas.Id, Quantity = 3, UnitPrice = 7m },
                },
            });
            db.Orders.Add(new Order
            {
                CustomerId = ana.Id,
                DeliveryAt = DateTime.Now.AddDays(2),
                Lines = { new OrderLine { ProductId = pan.Id, Quantity = 1, UnitPrice = 5m } },
            });
            await db.SaveChangesAsync();
        }

        await using var db2 = CreateDb(dbName);
        var service = BuildService(dbName);
        var (lines, orders) = await service.CountUsageAsync(1);
        Assert.Equal(2, orders);
        Assert.Equal(2, lines);

        var (lines2, orders2) = await service.CountUsageAsync(2);
        Assert.Equal(1, orders2);
        Assert.Equal(1, lines2);
    }

    [Fact]
    public async Task OrdersQuery_StillResolvesSoftDeletedProductName()
    {
        var dbName = nameof(OrdersQuery_StillResolvesSoftDeletedProductName);
        await using (var db = CreateDb(dbName))
        {
            var ana = new Customer { Name = "Ana" };
            var pan = new Product { Name = "Pan", PrepHours = 24, Price = 5m };
            db.AddRange(ana, pan);
            db.Orders.Add(new Order
            {
                CustomerId = ana.Id,
                DeliveryAt = DateTime.Now.AddDays(1),
                Lines = { new OrderLine { ProductId = pan.Id, Quantity = 1, UnitPrice = 5m } },
            });
            await db.SaveChangesAsync();
        }

        await using (var db2 = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.SoftDeleteAsync(1);
        }

        await using var db3 = CreateDb(dbName);
        var name = await db3.OrderLines
            .Include(l => l.Product)
            .Select(l => l.Product!.Name)
            .FirstAsync();
        Assert.Equal("Pan", name);
    }
}
