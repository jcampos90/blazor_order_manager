using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderManager.Web.Auth;
using OrderManager.Web.Data;
using OrderManager.Web.Models;

namespace OrderManager.Web.Services;

public enum OwnerAdmission
{
    Admitted,
    Denied,
}

public sealed class OwnerService(IDbContextFactory<AppDbContext> factory, IOptions<OwnerAuthOptions> options)
{
    /// <summary>
    /// Admits a signed-in Clerk user: while no owner exists and claiming is allowed, the
    /// first caller becomes the owner; thereafter only the owner is admitted.
    /// </summary>
    public async Task<OwnerAdmission> AdmitAsync(string clerkUserId, CancellationToken ct = default)
    {
        var ownerId = await GetOwnerIdAsync(ct);
        if (ownerId is not null)
            return ownerId == clerkUserId ? OwnerAdmission.Admitted : OwnerAdmission.Denied;

        if (!options.Value.AllowClaim)
            return OwnerAdmission.Denied;

        return await TryClaimAsync(clerkUserId, ct)
            ? OwnerAdmission.Admitted
            : OwnerAdmission.Denied;
    }

    public async Task<string?> GetOwnerIdAsync(CancellationToken ct = default)
    {
        await using var db = await factory.CreateDbContextAsync(ct);
        return (await db.AppOwners.FirstOrDefaultAsync(ct))?.ClerkUserId;
    }

    /// <summary>
    /// Records clerkUserId as the owner if none exists yet (first-write-wins). Returns
    /// true only when clerkUserId is the recorded owner afterward.
    /// </summary>
    public async Task<bool> TryClaimAsync(string clerkUserId, CancellationToken ct = default)
    {
        if (!options.Value.AllowClaim)
            return false;

        await using var db = await factory.CreateDbContextAsync(ct);
        if (await db.AppOwners.AnyAsync(ct))
            return (await db.AppOwners.FirstOrDefaultAsync(ct))?.ClerkUserId == clerkUserId;

        var owner = new AppOwner { Id = 1, ClerkUserId = clerkUserId };
        db.AppOwners.Add(owner);
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException)
        {
            db.Entry(owner).State = EntityState.Detached;
            return (await db.AppOwners.FirstOrDefaultAsync(ct))?.ClerkUserId == clerkUserId;
        }
    }
}