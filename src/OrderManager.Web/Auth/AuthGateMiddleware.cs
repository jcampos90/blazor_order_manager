namespace OrderManager.Web.Auth;

public sealed class AuthGateMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context) => next(context);
}
