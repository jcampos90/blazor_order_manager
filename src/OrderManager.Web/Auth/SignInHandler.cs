using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using OrderManager.Web.Models;

namespace OrderManager.Web.Auth;

public static class SignInHandler
{
    public static async Task<IResult> SignInAsync(
        HttpContext context,
        SignInManager<AppUser> signInManager,
        IAntiforgery antiforgery)
    {
        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString();
        var password = form["password"].ToString();
        var returnUrl = string.IsNullOrEmpty(form["returnUrl"]) ? "/" : form["returnUrl"].ToString();

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.Redirect(BuildLoginUrl(returnUrl, "invalid"));
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return Results.Redirect(BuildLoginUrl(returnUrl, "missing"));
        }

        var result = await signInManager.PasswordSignInAsync(
            email, password, isPersistent: true, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            var user = await signInManager.UserManager.FindByEmailAsync(email);
            if (user?.MustChangePassword == true)
            {
                return Results.LocalRedirect("/cuenta?force=1");
            }
            return Results.LocalRedirect(returnUrl);
        }

        return Results.Redirect(BuildLoginUrl(returnUrl, "invalid"));
    }

    private static string BuildLoginUrl(string returnUrl, string error) =>
        $"/login?error={error}&ReturnUrl={Uri.EscapeDataString(returnUrl)}";
}
