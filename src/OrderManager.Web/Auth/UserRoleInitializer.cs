using Microsoft.AspNetCore.Identity;
using OrderManager.Web.Models;

namespace OrderManager.Web.Auth;

public static class UserRoleInitializer
{
    public const string OwnerRole = "Owner";
    public const string StaffRole = "Staff";

    private const string AdminEmail = "admin@ordermanager.local";
    private const string AdminPassword = "Admin123!";

    public static async Task InitializeAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(OwnerRole))
        {
            await roleManager.CreateAsync(new IdentityRole(OwnerRole));
        }

        if (!await roleManager.RoleExistsAsync(StaffRole))
        {
            await roleManager.CreateAsync(new IdentityRole(StaffRole));
        }

        var user = await userManager.FindByEmailAsync(AdminEmail);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                DisplayName = "Admin",
                MustChangePassword = false,
            };
            var result = await userManager.CreateAsync(user, AdminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(user, OwnerRole))
        {
            await userManager.AddToRoleAsync(user, OwnerRole);
        }
    }
}
