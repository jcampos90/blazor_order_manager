using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrderManager.Web.Auth;

namespace OrderManager.Web.Tests;

public class SignOutHandlerTests
{
    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public List<string> SignedOutSchemes { get; } = [];

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
            => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
            => Task.CompletedTask;

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            SignedOutSchemes.Add(scheme!);
            return Task.CompletedTask;
        }
    }

    private static async Task<(FakeAuthenticationService auth, DefaultHttpContext context)> ActAndBuild()
    {
        var fake = new FakeAuthenticationService();
        var services = new ServiceCollection();
        services.AddSingleton<IAuthenticationService>(fake);
        var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext();
        context.RequestServices = provider;

        await SignOutHandler.SignOutAsync(context);
        return (fake, context);
    }

    [Fact]
    public async Task SignOutAsync_SignsOutIdentityApplicationScheme()
    {
        var (auth, _) = await ActAndBuild();

        Assert.Contains(IdentityConstants.ApplicationScheme, auth.SignedOutSchemes);
    }

    [Fact]
    public async Task SignOutAsync_DoesNotIssueItsOwnRedirect()
    {
        var (_, context) = await ActAndBuild();

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("Location"),
            "The handler must not set its own Location header.");
    }
}
