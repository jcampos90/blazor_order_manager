using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderManager.Web.Auth;
using OrderManager.Web.Data;
using OrderManager.Web.Services;

namespace OrderManager.Web.Tests;

public class OwnerServiceTests
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

    private static OwnerService CreateService(string dbName, bool allowClaim = true)
        => new(
            new TestFactory(() => CreateDb(dbName)),
            Options.Create(new OwnerAuthOptions { AllowClaim = allowClaim }));

    [Fact]
    public async Task AdmitAsync_NoOwnerAndClaimAllowed_ClaimsFirstSignInAsOwner()
    {
        var dbName = nameof(AdmitAsync_NoOwnerAndClaimAllowed_ClaimsFirstSignInAsOwner);
        var service = CreateService(dbName);

        var result = await service.AdmitAsync("clerk-user-1");

        Assert.Equal(OwnerAdmission.Admitted, result);
        Assert.Equal("clerk-user-1", await service.GetOwnerIdAsync());
    }

    [Fact]
    public async Task AdmitAsync_NoOwnerAndClaimDisabled_DeniesAndPersistsNothing()
    {
        var dbName = nameof(AdmitAsync_NoOwnerAndClaimDisabled_DeniesAndPersistsNothing);
        var service = CreateService(dbName, allowClaim: false);

        var result = await service.AdmitAsync("clerk-user-1");

        Assert.Equal(OwnerAdmission.Denied, result);
        Assert.Null(await service.GetOwnerIdAsync());
    }

    [Fact]
    public async Task AdmitAsync_OwnerExists_AdmitsTheOwner()
    {
        var dbName = nameof(AdmitAsync_OwnerExists_AdmitsTheOwner);
        var service = CreateService(dbName);
        await service.AdmitAsync("clerk-owner");

        var result = await service.AdmitAsync("clerk-owner");

        Assert.Equal(OwnerAdmission.Admitted, result);
    }

    [Fact]
    public async Task AdmitAsync_OwnerExists_DeniesOtherUsersAndKeepsOwner()
    {
        var dbName = nameof(AdmitAsync_OwnerExists_DeniesOtherUsersAndKeepsOwner);
        var service = CreateService(dbName);
        await service.AdmitAsync("clerk-owner");

        var result = await service.AdmitAsync("clerk-intruder");

        Assert.Equal(OwnerAdmission.Denied, result);
        Assert.Equal("clerk-owner", await service.GetOwnerIdAsync());
    }

    [Fact]
    public async Task TryClaimAsync_WhenOwnerAlreadyClaimed_DoesNotOverwrite()
    {
        var dbName = nameof(TryClaimAsync_WhenOwnerAlreadyClaimed_DoesNotOverwrite);
        var service = CreateService(dbName);
        await service.AdmitAsync("clerk-owner");

        var claimed = await service.TryClaimAsync("clerk-other");

        Assert.False(claimed);
        Assert.Equal("clerk-owner", await service.GetOwnerIdAsync());
    }

    [Fact]
    public async Task GetOwnerIdAsync_ReturnsNullWhenNobodyClaimed()
    {
        var dbName = nameof(GetOwnerIdAsync_ReturnsNullWhenNobodyClaimed);
        var service = CreateService(dbName);

        Assert.Null(await service.GetOwnerIdAsync());
    }
}