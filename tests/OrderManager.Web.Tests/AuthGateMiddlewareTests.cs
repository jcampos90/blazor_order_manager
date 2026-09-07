using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrderManager.Web.Auth;

namespace OrderManager.Web.Tests;

public class AuthGateMiddlewareTests
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

    private static (FakeAuthenticationService auth, DefaultHttpContext context, AuthGateMiddleware middleware) BuildPipeline(ClaimsPrincipal? user = null)
    {
        var fake = new FakeAuthenticationService();
        var services = new ServiceCollection();
        services.AddSingleton<IAuthenticationService>(fake);
        var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext { RequestServices = provider };
        if (user is not null)
            context.User = user;

        var middleware = new AuthGateMiddleware(_ => Task.CompletedTask);
        return (fake, context, middleware);
    }

    private static ClaimsPrincipal BuildAuthenticatedUser(params Claim[] extraClaims)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser"),
        };
        claims.AddRange(extraClaims);
        var identity = new ClaimsIdentity(claims, "TestScheme");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal BuildUnauthenticatedUser()
    {
        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedUserWithOwnerRole_PassesThrough()
    {
        var user = BuildAuthenticatedUser(new Claim(ClaimTypes.Role, "Owner"));
        var (auth, context, middleware) = BuildPipeline(user);

        await middleware.InvokeAsync(context);

        Assert.Empty(auth.SignedOutSchemes);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AuthenticatedStaffUser_PassesThrough()
    {
        var user = BuildAuthenticatedUser(new Claim(ClaimTypes.Role, "Staff"));
        var (auth, context, middleware) = BuildPipeline(user);

        await middleware.InvokeAsync(context);

        Assert.Empty(auth.SignedOutSchemes);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_UnauthenticatedUser_PassesThrough()
    {
        var user = BuildUnauthenticatedUser();
        var (auth, context, middleware) = BuildPipeline(user);

        await middleware.InvokeAsync(context);

        Assert.Empty(auth.SignedOutSchemes);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }
}
