using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;

namespace OrderManager.Web.Services;

public class OrderService(IDbContextFactory<AppDbContext> factory)
{
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        var order = await db.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
            return;
        if (order.Status == OrderStatus.Delivered)
            throw new InvalidOperationException("No se puede eliminar un encargo entregado.");

        db.Orders.Remove(order);
        await db.SaveChangesAsync(ct);
    }
}
