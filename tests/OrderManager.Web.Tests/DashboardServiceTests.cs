using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;
using OrderManager.Web.Services;

namespace OrderManager.Web.Tests;

public class DashboardServiceTests
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

    [Fact]
    public async Task GetProductionAsync_ExcludesDeliveredAndPast_ComputesStartBy()
    {
        var dbName = nameof(GetProductionAsync_ExcludesDeliveredAndPast_ComputesStartBy);
        await using (var db = CreateDb(dbName))
        {
            var customer = new Customer { Name = "Juan" };
            var pan = new Product { Name = "Pan", PrepHours = 24, Price = 5m };
            db.AddRange(customer, pan);

            db.Orders.Add(new Order
            {
                Customer = customer,
                DeliveryAt = new DateTime(2026, 8, 15, 8, 0, 0),
                Status = OrderStatus.Pending,
                Lines =
                {
                    new OrderLine { Product = pan, Quantity = 2, UnitPrice = 5m },
                },
            });

            db.Orders.Add(new Order
            {
                Customer = customer,
                DeliveryAt = new DateTime(2026, 8, 15, 8, 0, 0),
                Status = OrderStatus.Delivered,
                Lines =
                {
                    new OrderLine { Product = pan, Quantity = 1, UnitPrice = 5m },
                },
            });

            db.Orders.Add(new Order
            {
                Customer = customer,
                DeliveryAt = new DateTime(2026, 7, 1, 8, 0, 0),
                Status = OrderStatus.Pending,
                Lines =
                {
                    new OrderLine { Product = pan, Quantity = 1, UnitPrice = 5m },
                },
            });

            await db.SaveChangesAsync();
        }

        await using (var db2 = CreateDb(dbName))
        {
            var service = new DashboardService(new TestFactory(() => CreateDb(dbName)));
            var now = new DateTime(2026, 8, 14, 9, 0, 0);
            var items = await service.GetProductionAsync(now);

            var item = Assert.Single(items);
            Assert.Equal(new DateTime(2026, 8, 14, 8, 0, 0), item.StartBy);
            Assert.Equal(2, item.Quantity);
            Assert.True(item.Overdue(now));
        }
    }

    [Fact]
    public async Task GetProductionAsync_OrdersByStartByAscending()
    {
        var dbName = nameof(GetProductionAsync_OrdersByStartByAscending);
        await using (var db = CreateDb(dbName))
        {
            var customer = new Customer { Name = "Ana" };
            var pan = new Product { Name = "Pan", PrepHours = 24, Price = 5m };
            var facturas = new Product { Name = "Facturas", PrepHours = 8, Price = 7m };
            db.AddRange(customer, pan, facturas);

            db.Orders.Add(new Order
            {
                Customer = customer,
                DeliveryAt = new DateTime(2026, 8, 15, 8, 0, 0),
                Lines = { new OrderLine { Product = pan, Quantity = 1, UnitPrice = 5m } },
            });
            db.Orders.Add(new Order
            {
                Customer = customer,
                DeliveryAt = new DateTime(2026, 8, 15, 12, 0, 0),
                Lines = { new OrderLine { Product = facturas, Quantity = 1, UnitPrice = 7m } },
            });
            await db.SaveChangesAsync();
        }

        await using var db2 = CreateDb(dbName);
        var service = new DashboardService(new TestFactory(() => CreateDb(dbName)));
        var items = await service.GetProductionAsync(new DateTime(2026, 8, 14, 9, 0, 0));

        Assert.Equal(2, items.Count);
        Assert.Equal(new DateTime(2026, 8, 14, 8, 0, 0), items[0].StartBy);
        Assert.Equal(new DateTime(2026, 8, 15, 4, 0, 0), items[1].StartBy);
    }
}
