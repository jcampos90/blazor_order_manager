using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace OrderManager.Web.Auth;

/// <summary>
/// Ends the visitor's session: clears the local auth cookie and signs out of the Clerk
/// OIDC session. The OIDC scheme's sign-out redirects to Clerk's end-session endpoint
/// (remote end-session), which then returns the visitor to the app's signed-out callback
/// path; the fallback authorization policy bounces them to sign-in. No manual redirect is
/// issued here, because doing so would overwrite the OIDC handler's end-session redirect.
/// </summary>
public static class SignOutHandler
{
    public static async Task SignOutAsync(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }
}
