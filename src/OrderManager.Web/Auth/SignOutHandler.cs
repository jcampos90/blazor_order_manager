using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace OrderManager.Web.Auth;

public static class SignOutHandler
{
    public static async Task SignOutAsync(HttpContext context)
    {
        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
    }
}
