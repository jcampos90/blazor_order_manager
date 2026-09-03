using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace OrderManager.Web.Auth;

public sealed class OwnerGateMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true
            && !context.User.IsInRole("Owner"))
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            context.Response.Redirect("/access-denied");
            return;
        }

        await next(context);
    }
}
