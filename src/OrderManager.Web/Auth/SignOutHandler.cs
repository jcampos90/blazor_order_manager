using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace OrderManager.Web.Auth;

public static class SignOutHandler
{
    public static async Task<IResult> SignOutAsync(HttpContext context, IAntiforgery antiforgery)
    {
        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            return Results.Redirect("/");
        }

        await context.SignOutAsync(IdentityConstants.ApplicationScheme);
        return Results.Redirect("/login");
    }
}
