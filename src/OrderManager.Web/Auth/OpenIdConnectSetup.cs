using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace OrderManager.Web.Auth;

public static class OpenIdConnectSetup
{
    public static void Configure(ClerkAuthOptions options, OpenIdConnectOptions oidc)
    {
        oidc.Authority = options.Authority;
        oidc.ClientId = options.ClientId;
        oidc.ClientSecret = options.ClientSecret;
        oidc.ResponseType = OpenIdConnectResponseType.Code;
        oidc.GetClaimsFromUserInfoEndpoint = true;
        oidc.MapInboundClaims = false;
        oidc.SaveTokens = true;
        oidc.TokenValidationParameters.NameClaimType = "name";

        oidc.Scope.Clear();
        oidc.Scope.Add("openid");
        oidc.Scope.Add("profile");
        oidc.Scope.Add("email");
    }
}