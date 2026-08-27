using Microsoft.Extensions.Configuration;
using OrderManager.Web.Auth;

namespace OrderManager.Web.Tests;

public class ClerkAuthOptionsTests
{
    [Fact]
    public void SectionName_PointsAtAuthOidc()
    {
        Assert.Equal("Auth:Oidc", ClerkAuthOptions.SectionName);
    }

    [Fact]
    public void Bind_ReadsOidcSectionFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:Oidc:Authority"] = "https://bakery.clerk.accounts.dev",
                ["Auth:Oidc:ClientId"] = "client-id",
                ["Auth:Oidc:ClientSecret"] = "client-secret",
                ["Auth:Oidc:PublishableKey"] = "pk_test_x",
            })
            .Build();

        var options = configuration
            .GetSection(ClerkAuthOptions.SectionName)
            .Get<ClerkAuthOptions>();

        Assert.NotNull(options);
        Assert.Equal("https://bakery.clerk.accounts.dev", options!.Authority);
        Assert.Equal("client-id", options.ClientId);
        Assert.Equal("client-secret", options.ClientSecret);
        Assert.Equal("pk_test_x", options.PublishableKey);
    }
}