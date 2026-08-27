using Microsoft.Extensions.Configuration;
using OrderManager.Web.Auth;

namespace OrderManager.Web.Tests;

public class OwnerAuthOptionsTests
{
    [Fact]
    public void SectionName_PointsAtAuth()
    {
        Assert.Equal("Auth", OwnerAuthOptions.SectionName);
    }

    [Fact]
    public void AllowClaim_DefaultsTrue()
    {
        Assert.True(new OwnerAuthOptions().AllowClaim);
    }

    [Fact]
    public void Bind_ReadsAllowClaimFromConfig()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Auth:AllowClaim"] = "false",
            })
            .Build();

        var options = configuration
            .GetSection(OwnerAuthOptions.SectionName)
            .Get<OwnerAuthOptions>();

        Assert.NotNull(options);
        Assert.False(options!.AllowClaim);
    }
}