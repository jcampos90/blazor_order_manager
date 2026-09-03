using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderManager.Web.Auth;
using OrderManager.Web.Components;
using OrderManager.Web.Data;
using OrderManager.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
});

builder.Services.AddScoped<DashboardService>();

var authOptions = builder.Configuration
    .GetSection(ClerkAuthOptions.SectionName)
    .Get<ClerkAuthOptions>() ?? new ClerkAuthOptions();

builder.Services.Configure<OwnerAuthOptions>(
    builder.Configuration.GetSection(OwnerAuthOptions.SectionName));

var ownerOptions = builder.Configuration
    .GetSection(OwnerAuthOptions.SectionName)
    .Get<OwnerAuthOptions>() ?? new OwnerAuthOptions();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o => o.Cookie.SecurePolicy = CookieSecurePolicy.Always)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, oidc =>
    OpenIdConnectSetup.Configure(authOptions, oidc));

builder.Services.AddAuthorization(AuthorizationSetup.Configure);
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<OwnerService>();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
                      | ForwardedHeaders.XForwardedProto
                      | ForwardedHeaders.XForwardedHost,
});

if (string.IsNullOrWhiteSpace(authOptions.Authority)
    || string.IsNullOrWhiteSpace(authOptions.ClientId)
    || string.IsNullOrWhiteSpace(authOptions.ClientSecret))
{
    throw new InvalidOperationException(
        "Clerk OIDC is not configured. Set Auth:Oidc:Authority, Auth:Oidc:ClientId and " +
        "Auth:Oidc:ClientSecret via `dotnet user-secrets set` before starting the app.");
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseMiddleware<OwnerGateMiddleware>();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapPost("/signout", (HttpContext context) => SignOutHandler.SignOutAsync(context))
    .RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await DbSeeder.SeedAsync(factory);

    if (!ownerOptions.AllowClaim)
    {
        var ownerService = scope.ServiceProvider.GetRequiredService<OwnerService>();
        if (await ownerService.GetOwnerIdAsync() is null)
        {
            throw new InvalidOperationException(
                "Auth:AllowClaim is disabled but no owner is recorded. Enable Auth:AllowClaim " +
                "and sign in once to claim the owner, then disable it for production.");
        }
    }
}

app.Run();
