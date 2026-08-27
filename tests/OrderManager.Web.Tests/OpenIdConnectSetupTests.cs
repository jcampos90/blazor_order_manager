using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OrderManager.Web.Auth;

namespace OrderManager.Web.Tests;

public class OpenIdConnectSetupTests
{
    private static OpenIdConnectOptions BuildConfiguredOptions(ClerkAuthOptions? options = null)
    {
        var oidc = new OpenIdConnectOptions();
        OpenIdConnectSetup.Configure(options ?? new ClerkAuthOptions(), oidc);
        return oidc;
    }

    [Fact]
    public void Configure_AppliesClerkCredentialsAndAuthority()
    {
        var options = new ClerkAuthOptions
        {
            Authority = "https://bakery.clerk.accounts.dev",
            ClientId = "client-id",
            ClientSecret = "client-secret",
        };

        var oidc = BuildConfiguredOptions(options);

        Assert.Equal("https://bakery.clerk.accounts.dev", oidc.Authority);
        Assert.Equal("client-id", oidc.ClientId);
        Assert.Equal("client-secret", oidc.ClientSecret);
    }

    [Fact]
    public void Configure_UsesAuthorizationCodeFlowWithPkce()
    {
        var oidc = BuildConfiguredOptions();

        Assert.Equal(OpenIdConnectResponseType.Code, oidc.ResponseType);
        Assert.True(oidc.UsePkce);
    }

    [Fact]
    public void Configure_RequestsOpenIdProfileAndEmailScopes()
    {
        var oidc = BuildConfiguredOptions();

        Assert.Equal(new[] { "openid", "profile", "email" }, oidc.Scope);
    }

    [Fact]
    public void Configure_EnablesUserInfoEndpoint()
    {
        var oidc = BuildConfiguredOptions();

        Assert.True(oidc.GetClaimsFromUserInfoEndpoint);
    }

    [Fact]
    public void Configure_KeepsInboundClaimsUnmapped_SoSubSurvivesAsSub()
    {
        var oidc = BuildConfiguredOptions();

        Assert.False(oidc.MapInboundClaims);
        Assert.Equal("name", oidc.TokenValidationParameters.NameClaimType);
    }
}