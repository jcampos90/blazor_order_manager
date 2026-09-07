using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;
using OrderManager.Web.Services;

namespace OrderManager.Web.Tests;

public class PapeleraTests
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
    public async Task SoftDeletedCustomer_AppearsInPapeleraQuery_AndRestoreRemovesIt()
    {
        var dbName = nameof(SoftDeletedCustomer_AppearsInPapeleraQuery_AndRestoreRemovesIt);
        await using (var db = CreateDb(dbName))
        {
            db.Customers.Add(new Customer { Name = "Ana", DeletedAt = DateTime.UtcNow });
            db.Customers.Add(new Customer { Name = "Beto" });
            await db.SaveChangesAsync();
        }

        await using (var db2 = CreateDb(dbName))
        {
            var customers = new CustomerService(new TestFactory(() => CreateDb(dbName)));
            var deleted = await customers.GetDeletedAsync();
            Assert.Equal(new[] { "Ana" }, deleted.Select(c => c.Name).ToArray());

            await customers.RestoreAsync(1);
        }

        await using var db3 = CreateDb(dbName);
        var customers2 = new CustomerService(new TestFactory(() => CreateDb(dbName)));
        Assert.Empty(await customers2.GetDeletedAsync());
        var ana = await db3.Customers.AsNoTracking().FirstAsync(c => c.Id == 1);
        Assert.Null(ana.DeletedAt);
    }

    [Fact]
    public async Task SoftDeletedProduct_AppearsInPapeleraQuery_AndRestoreRemovesIt()
    {
        var dbName = nameof(SoftDeletedProduct_AppearsInPapeleraQuery_AndRestoreRemovesIt);
        await using (var db = CreateDb(dbName))
        {
            db.Products.Add(new Product { Name = "Pan", PrepHours = 24, Price = 5m, DeletedAt = DateTime.UtcNow });
            db.Products.Add(new Product { Name = "Facturas", PrepHours = 8, Price = 7m });
            await db.SaveChangesAsync();
        }

        await using (var db2 = CreateDb(dbName))
        {
            var products = new ProductService(new TestFactory(() => CreateDb(dbName)));
            var deleted = await products.GetDeletedAsync();
            Assert.Equal(new[] { "Pan" }, deleted.Select(p => p.Name).ToArray());

            await products.RestoreAsync(1);
        }

        await using var db3 = CreateDb(dbName);
        var products2 = new ProductService(new TestFactory(() => CreateDb(dbName)));
        Assert.Empty(await products2.GetDeletedAsync());
        var pan = await db3.Products.AsNoTracking().FirstAsync(p => p.Id == 1);
        Assert.Null(pan.DeletedAt);
    }
}
