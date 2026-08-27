using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;

namespace OrderManager.Web.Services;

public static class DbSeeder
{
    public static async Task SeedAsync(IDbContextFactory<AppDbContext> factory)
    {
        await using var db = await factory.CreateDbContextAsync();
        if (await db.Products.AnyAsync())
            return;

        db.Products.AddRange(
            new Product { Name = "Pan blanco (1 kg)", PrepHours = 24, Price = 4.5m },
            new Product { Name = "Pan integral (1 kg)", PrepHours = 30, Price = 5.0m },
            new Product { Name = "Pan de campo (1 kg)", PrepHours = 48, Price = 6.0m },
            new Product { Name = "Facturas (docena)", PrepHours = 8, Price = 7.0m },
            new Product { Name = "Baguette", PrepHours = 6, Price = 2.0m });

        await db.SaveChangesAsync();
    }
}
