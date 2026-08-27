using Microsoft.AspNetCore.Authorization;

namespace OrderManager.Web.Auth;

public static class AuthorizationSetup
{
    public static void Configure(AuthorizationOptions options)
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    }
}