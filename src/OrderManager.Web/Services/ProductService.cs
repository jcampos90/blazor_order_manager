using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;

namespace OrderManager.Web.Services;

public class ProductService(IDbContextFactory<AppDbContext> factory)
{
    public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return;
        product.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return;
        product.DeletedAt = null;
        await db.SaveChangesAsync(ct);
    }

    public async Task ToggleActiveAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null)
            return;
        product.IsActive = !product.IsActive;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Products
            .Where(p => p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetOfferedAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Products
            .Where(p => p.IsActive && p.DeletedAt == null)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetDeletedAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Products
            .Where(p => p.DeletedAt != null)
            .OrderBy(p => p.Name)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<(int Lines, int Orders)> CountUsageAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var lines = await db.OrderLines.CountAsync(l => l.ProductId == id, ct);
        var orders = await db.Orders.CountAsync(o => o.Lines.Any(l => l.ProductId == id), ct);
        return (lines, orders);
    }
}
