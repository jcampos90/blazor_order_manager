using Microsoft.AspNetCore.Identity;

namespace OrderManager.Web.Auth;

public static class SignInHandler
{
    public static async Task<IResult> SignInAsync(
        HttpContext context,
        SignInManager<IdentityUser> signInManager)
    {
        var form = await context.Request.ReadFormAsync();
        var email = form["email"].ToString();
        var password = form["password"].ToString();
        var returnUrl = string.IsNullOrEmpty(form["returnUrl"]) ? "/" : form["returnUrl"].ToString();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return Results.Redirect(BuildLoginUrl(returnUrl, "missing"));
        }

        var result = await signInManager.PasswordSignInAsync(
            email, password, isPersistent: true, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return Results.LocalRedirect(returnUrl);
        }

        return Results.Redirect(BuildLoginUrl(returnUrl, "invalid"));
    }

    private static string BuildLoginUrl(string returnUrl, string error) =>
        $"/login?error={error}&ReturnUrl={Uri.EscapeDataString(returnUrl)}";
}
