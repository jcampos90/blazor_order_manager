using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using OrderManager.Web.Auth;

namespace OrderManager.Web.Tests;

public class AuthorizationSetupTests
{
    [Fact]
    public void Configure_SetsFallbackPolicyRequiringAuthentication()
    {
        var options = new AuthorizationOptions();

        AuthorizationSetup.Configure(options);

        Assert.NotNull(options.FallbackPolicy);
        Assert.Contains(
            options.FallbackPolicy!.Requirements,
            requirement => requirement is DenyAnonymousAuthorizationRequirement);
    }
}