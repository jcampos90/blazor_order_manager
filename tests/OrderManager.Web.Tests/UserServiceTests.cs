using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManager.Web.Auth;
using OrderManager.Web.Data;
using OrderManager.Web.Models;
using OrderManager.Web.Services;

namespace OrderManager.Web.Tests;

public class UserServiceTests
{
    private static (UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ServiceProvider provider) BuildServices(string dbName)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddIdentity<AppUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 1;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        var provider = services.BuildServiceProvider();
        return (
            provider.GetRequiredService<UserManager<AppUser>>(),
            provider.GetRequiredService<RoleManager<IdentityRole>>(),
            provider);
    }

    private static UserService BuildService(string dbName)
    {
        var (um, rm, _) = BuildServices(dbName);
        return new UserService(um, rm);
    }

    private static async Task<AppUser> SeedOwnerAsync(UserManager<AppUser> um, RoleManager<IdentityRole> rm, string email = "owner@test.local")
    {
        if (!await rm.RoleExistsAsync("Owner"))
            await rm.CreateAsync(new IdentityRole("Owner"));
        if (!await rm.RoleExistsAsync("Staff"))
            await rm.CreateAsync(new IdentityRole("Staff"));
        var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await um.CreateAsync(user, "Password123!");
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        await um.AddToRoleAsync(user, "Owner");
        return user;
    }

    [Fact]
    public async Task CreateAsync_ReturnsTempPassword_AndCreatesUserWithStaffRole()
    {
        var dbName = nameof(CreateAsync_ReturnsTempPassword_AndCreatesUserWithStaffRole);
        var (um, rm, _) = BuildServices(dbName);
        await SeedOwnerAsync(um, rm);

        var service = new UserService(um, rm);
        var (temp, userId) = await service.CreateAsync("staff@test.local", "Pérez", "Staff");

        Assert.False(string.IsNullOrEmpty(temp));
        Assert.True(temp.Length >= 12);

        var created = await um.FindByIdAsync(userId);
        Assert.NotNull(created);
        Assert.Equal("Pérez", created!.DisplayName);
        Assert.True(created.MustChangePassword);
        Assert.True(await um.IsInRoleAsync(created, "Staff"));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEmailAlreadyTaken()
    {
        var dbName = nameof(CreateAsync_Throws_WhenEmailAlreadyTaken);
        var (um, rm, _) = BuildServices(dbName);
        await SeedOwnerAsync(um, rm);

        var service = new UserService(um, rm);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync("owner@test.local", null, "Staff"));
    }

    [Fact]
    public async Task ChangeRoleAsync_Throws_LastOwnerException_WhenDemotingLastOwner()
    {
        var dbName = nameof(ChangeRoleAsync_Throws_LastOwnerException_WhenDemotingLastOwner);
        var (um, rm, _) = BuildServices(dbName);
        var owner = await SeedOwnerAsync(um, rm);

        var service = new UserService(um, rm);
        await Assert.ThrowsAsync<LastOwnerException>(() =>
            service.ChangeRoleAsync(owner.Id, "Staff"));
    }

    [Fact]
    public async Task ChangeRoleAsync_AllowsDemotingOwner_WhenAnotherOwnerExists()
    {
        var dbName = nameof(ChangeRoleAsync_AllowsDemotingOwner_WhenAnotherOwnerExists);
        var (um, rm, _) = BuildServices(dbName);
        var first = await SeedOwnerAsync(um, rm, "a@test.local");
        var second = await SeedOwnerAsync(um, rm, "b@test.local");

        var service = new UserService(um, rm);
        await service.ChangeRoleAsync(first.Id, "Staff");

        Assert.True(await um.IsInRoleAsync(second, "Owner"));
        Assert.False(await um.IsInRoleAsync(first, "Owner"));
        Assert.True(await um.IsInRoleAsync(first, "Staff"));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTargetIsLastOwner()
    {
        var dbName = nameof(DeleteAsync_ReturnsFalse_WhenTargetIsLastOwner);
        var (um, rm, _) = BuildServices(dbName);
        var owner = await SeedOwnerAsync(um, rm);

        var service = new UserService(um, rm);
        var ok = await service.DeleteAsync(owner.Id);

        Assert.False(ok);
        Assert.NotNull(await um.FindByIdAsync(owner.Id));
    }

    [Fact]
    public async Task DeleteAsync_RemovesStaffUser()
    {
        var dbName = nameof(DeleteAsync_RemovesStaffUser);
        var (um, rm, _) = BuildServices(dbName);
        await SeedOwnerAsync(um, rm);

        var service = new UserService(um, rm);
        var (_, staffId) = await service.CreateAsync("staff@test.local", null, "Staff");

        var ok = await service.DeleteAsync(staffId);

        Assert.True(ok);
        Assert.Null(await um.FindByIdAsync(staffId));
    }

    [Fact]
    public async Task ResetPasswordAsync_MarksUserWithMustChangePassword_AndReturnsNewTemp()
    {
        var dbName = nameof(ResetPasswordAsync_MarksUserWithMustChangePassword_AndReturnsNewTemp);
        var (um, rm, _) = BuildServices(dbName);
        var owner = await SeedOwnerAsync(um, rm);
        owner.MustChangePassword = false;
        await um.UpdateAsync(owner);

        var service = new UserService(um, rm);
        var temp = await service.ResetPasswordAsync(owner.Id);

        Assert.False(string.IsNullOrEmpty(temp));
        var refreshed = await um.FindByIdAsync(owner.Id);
        Assert.True(refreshed!.MustChangePassword);

        var signIn = await um.CheckPasswordAsync(refreshed, temp);
        Assert.True(signIn);
    }

    [Fact]
    public async Task ChangeOwnPasswordAsync_RejectsWrongCurrentPassword()
    {
        var dbName = nameof(ChangeOwnPasswordAsync_RejectsWrongCurrentPassword);
        var (um, rm, _) = BuildServices(dbName);
        var owner = await SeedOwnerAsync(um, rm);

        var service = new UserService(um, rm);
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ChangeOwnPasswordAsync(owner.Id, "wrong-current", "NewPass123!"));
        Assert.Contains("actual", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangeOwnPasswordAsync_AcceptsCorrectCurrent_AndFlipsMustChangePassword()
    {
        var dbName = nameof(ChangeOwnPasswordAsync_AcceptsCorrectCurrent_AndFlipsMustChangePassword);
        var (um, rm, _) = BuildServices(dbName);
        var owner = await SeedOwnerAsync(um, rm);
        owner.MustChangePassword = true;
        await um.UpdateAsync(owner);

        var service = new UserService(um, rm);
        await service.ChangeOwnPasswordAsync(owner.Id, "Password123!", "BrandNew123!");

        var refreshed = await um.FindByIdAsync(owner.Id);
        Assert.False(refreshed!.MustChangePassword);
        Assert.True(await um.CheckPasswordAsync(refreshed, "BrandNew123!"));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenChangingEmailToExistingOne()
    {
        var dbName = nameof(UpdateAsync_Throws_WhenChangingEmailToExistingOne);
        var (um, rm, _) = BuildServices(dbName);
        await SeedOwnerAsync(um, rm);

        var service = new UserService(um, rm);
        var (_, staffId) = await service.CreateAsync("staff@test.local", null, "Staff");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(staffId, "owner@test.local", null));
    }

    [Fact]
    public async Task UpdateOwnDisplayNameAsync_SetsTrimmedDisplayName()
    {
        var dbName = nameof(UpdateOwnDisplayNameAsync_SetsTrimmedDisplayName);
        var (um, rm, _) = BuildServices(dbName);
        var owner = await SeedOwnerAsync(um, rm);

        var service = new UserService(um, rm);
        await service.UpdateOwnDisplayNameAsync(owner.Id, "  María  ");

        var refreshed = await um.FindByIdAsync(owner.Id);
        Assert.Equal("María", refreshed!.DisplayName);
    }
}
