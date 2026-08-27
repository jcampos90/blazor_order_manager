using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Data;
using OrderManager.Web.Models;

namespace OrderManager.Web.Services;

public sealed record PrepItem(
    int OrderLineId,
    string ProductName,
    decimal Quantity,
    string CustomerName,
    string CustomerPhone,
    DateTime DeliveryAt,
    int PrepHours,
    DateTime StartBy)
{
    public bool Overdue(DateTime now) => PrepSchedule.IsOverdue(StartBy, now);

    public UrgencyBucket Bucket(DateTime now) => UrgencyBuckets.Classify(StartBy, now);
}

public class DashboardService(IDbContextFactory<AppDbContext> factory)
{
    public async Task<IReadOnlyList<PrepItem>> GetProductionAsync(
        DateTime now, CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);

        var dayStart = now.Date;
        var raw = await db.Orders
            .Where(o => o.Status != OrderStatus.Delivered)
            .Where(o => o.DeliveryAt >= dayStart)
            .SelectMany(o => o.Lines.Select(l => new
            {
                l.Id,
                l.Quantity,
                ProductName = l.Product!.Name,
                ProductPrep = l.Product!.PrepHours,
                o.DeliveryAt,
                CustomerName = o.Customer!.Name,
                CustomerPhone = o.Customer!.Phone,
            }))
            .ToListAsync(ct);

        return raw
            .Select(r => new PrepItem(
                r.Id, r.ProductName, r.Quantity, r.CustomerName, r.CustomerPhone!,
                r.DeliveryAt, r.ProductPrep,
                PrepSchedule.StartBy(r.DeliveryAt, r.ProductPrep)))
            .OrderBy(p => p.StartBy)
            .ToList();
    }
}
