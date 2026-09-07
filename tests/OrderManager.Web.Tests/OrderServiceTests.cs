using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;
using OrderManager.Web.Services;

namespace OrderManager.Web.Tests;

public class OrderServiceTests
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

    private static OrderService BuildService(string dbName) =>
        new(new TestFactory(() => CreateDb(dbName)));

    private static async Task SeedOrderAsync(string dbName, OrderStatus status)
    {
        await using var db = CreateDb(dbName);
        var customer = new Customer { Name = "Ana" };
        var product = new Product { Name = "Pan", PrepHours = 24, Price = 5m };
        db.AddRange(customer, product);
        db.Orders.Add(new Order
        {
            CustomerId = customer.Id,
            DeliveryAt = new DateTime(2026, 12, 1, 8, 0, 0),
            Status = status,
            Lines =
            {
                new OrderLine { ProductId = product.Id, Quantity = 2, UnitPrice = 5m },
                new OrderLine { ProductId = product.Id, Quantity = 1, UnitPrice = 5m },
            },
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteAsync_PendingOrder_RemovesOrderAndCascadesLines()
    {
        var dbName = nameof(DeleteAsync_PendingOrder_RemovesOrderAndCascadesLines);
        await SeedOrderAsync(dbName, OrderStatus.Pending);

        await using (var db = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.DeleteAsync(1);
        }

        await using var db2 = CreateDb(dbName);
        Assert.Empty(await db2.Orders.ToListAsync());
        Assert.Empty(await db2.OrderLines.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_InProductionOrder_RemovesOrderAndCascadesLines()
    {
        var dbName = nameof(DeleteAsync_InProductionOrder_RemovesOrderAndCascadesLines);
        await SeedOrderAsync(dbName, OrderStatus.InProduction);

        await using (var db = CreateDb(dbName))
        {
            var service = BuildService(dbName);
            await service.DeleteAsync(1);
        }

        await using var db2 = CreateDb(dbName);
        Assert.Empty(await db2.Orders.ToListAsync());
        Assert.Empty(await db2.OrderLines.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_DeliveredOrder_Throws()
    {
        var dbName = nameof(DeleteAsync_DeliveredOrder_Throws);
        await SeedOrderAsync(dbName, OrderStatus.Delivered);

        await using var db = CreateDb(dbName);
        var service = BuildService(dbName);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync(1));
        Assert.Contains("entregado", ex.Message, StringComparison.CurrentCultureIgnoreCase);

        Assert.NotEmpty(await db.Orders.ToListAsync());
        Assert.NotEmpty(await db.OrderLines.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_UnknownId_IsNoOp()
    {
        var dbName = nameof(DeleteAsync_UnknownId_IsNoOp);
        await using var db = CreateDb(dbName);
        var service = BuildService(dbName);
        await service.DeleteAsync(42);
        Assert.Empty(await db.Orders.ToListAsync());
    }
}
