using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
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

    private sealed class PassThroughAntiforgery : IAntiforgery
    {
        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext) => new(null!, null!, null!, null!);
        public AntiforgeryTokenSet GetTokens(HttpContext httpContext) => new(null!, null!, null!, null!);
        public Task<bool> IsRequestValidAsync(HttpContext httpContext) => Task.FromResult(true);
        public void SetCookieTokenAndHeader(HttpContext httpContext) { }
        public Task ValidateRequestAsync(HttpContext httpContext) => Task.CompletedTask;
    }

    private static async Task<(FakeAuthenticationService auth, DefaultHttpContext context)> ActAndBuild()
    {
        var fake = new FakeAuthenticationService();
        var services = new ServiceCollection();
        services.AddSingleton<IAuthenticationService>(fake);
        services.AddSingleton<IAntiforgery>(new PassThroughAntiforgery());
        var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext();
        context.RequestServices = provider;

        await SignOutHandler.SignOutAsync(context, provider.GetRequiredService<IAntiforgery>());
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
