using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderManager.Web.Auth;
using OrderManager.Web.Data;

namespace OrderManager.Web.Tests;

public class OwnerRoleInitializerTests
{
    private static (UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ServiceProvider provider) BuildServices(string dbName)
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.AddIdentity<IdentityUser, IdentityRole>(options =>
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
            provider.GetRequiredService<UserManager<IdentityUser>>(),
            provider.GetRequiredService<RoleManager<IdentityRole>>(),
            provider);
    }

    [Fact]
    public async Task InitializeAsync_SeedsOwnerRole_WhenNotExists()
    {
        var dbName = nameof(InitializeAsync_SeedsOwnerRole_WhenNotExists);
        var (userManager, roleManager, _) = BuildServices(dbName);

        await OwnerRoleInitializer.InitializeAsync(userManager, roleManager);

        Assert.True(await roleManager.RoleExistsAsync("Owner"));
    }

    [Fact]
    public async Task InitializeAsync_CreatesAdminUser_WhenNotExists()
    {
        var dbName = nameof(InitializeAsync_CreatesAdminUser_WhenNotExists);
        var (userManager, roleManager, _) = BuildServices(dbName);

        await OwnerRoleInitializer.InitializeAsync(userManager, roleManager);

        var user = await userManager.FindByEmailAsync("admin@ordermanager.local");
        Assert.NotNull(user);
    }

    [Fact]
    public async Task InitializeAsync_AssignsOwnerRoleToAdminUser()
    {
        var dbName = nameof(InitializeAsync_AssignsOwnerRoleToAdminUser);
        var (userManager, roleManager, _) = BuildServices(dbName);

        await OwnerRoleInitializer.InitializeAsync(userManager, roleManager);

        var user = await userManager.FindByEmailAsync("admin@ordermanager.local");
        Assert.NotNull(user);
        Assert.True(await userManager.IsInRoleAsync(user!, "Owner"));
    }

    [Fact]
    public async Task InitializeAsync_Idempotent_CanRunTwiceWithoutError()
    {
        var dbName = nameof(InitializeAsync_Idempotent_CanRunTwiceWithoutError);
        var (userManager, roleManager, _) = BuildServices(dbName);

        await OwnerRoleInitializer.InitializeAsync(userManager, roleManager);
        await OwnerRoleInitializer.InitializeAsync(userManager, roleManager);

        var user = await userManager.FindByEmailAsync("admin@ordermanager.local");
        Assert.NotNull(user);
        Assert.True(await userManager.IsInRoleAsync(user!, "Owner"));
    }
}
