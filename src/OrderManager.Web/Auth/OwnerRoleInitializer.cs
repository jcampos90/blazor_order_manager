using Microsoft.AspNetCore.Identity;

namespace OrderManager.Web.Auth;

public static class OwnerRoleInitializer
{
    private const string OwnerRole = "Owner";
    private const string AdminEmail = "admin@ordermanager.local";
    private const string AdminPassword = "Admin123!";

    public static async Task InitializeAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        if (!await roleManager.RoleExistsAsync(OwnerRole))
        {
            await roleManager.CreateAsync(new IdentityRole(OwnerRole));
        }

        var user = await userManager.FindByEmailAsync(AdminEmail);
        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
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
