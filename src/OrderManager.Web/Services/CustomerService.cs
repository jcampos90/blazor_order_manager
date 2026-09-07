using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;

namespace OrderManager.Web.Services;

public class CustomerService(IDbContextFactory<AppDbContext> factory)
{
    public async Task SoftDeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (customer is null)
            return;
        customer.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (customer is null)
            return;
        customer.DeletedAt = null;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> GetActiveAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Customers
            .Where(c => c.DeletedAt == null)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Customer>> GetDeletedAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Customers
            .Where(c => c.DeletedAt != null)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<int> CountOrdersAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return await db.Orders.CountAsync(o => o.CustomerId == id, ct);
    }
}
