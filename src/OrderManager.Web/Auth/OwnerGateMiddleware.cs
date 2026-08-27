using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OrderManager.Web.Services;

namespace OrderManager.Web.Auth;

/// <summary>
/// Runs after authentication, before authorization. For an authenticated user who is not
/// the recorded owner, signs them out and sends them to the access-denied page instead of
/// bouncing them back to sign-in in a loop.
/// </summary>
public sealed class OwnerGateMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, OwnerService ownerService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var sub = context.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrWhiteSpace(sub)
                && await ownerService.AdmitAsync(sub) == OwnerAdmission.Denied)
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Response.Redirect("/access-denied");
                return;
            }
        }

        await next(context);
    }
}