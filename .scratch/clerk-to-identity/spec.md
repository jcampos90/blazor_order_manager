# Replace Clerk with ASP.NET Core Identity

Status: ready-for-agent

## Problem Statement

The app currently uses Clerk as an external identity provider, integrated via ASP.NET Core OpenID Connect middleware. Clerk is a third-party SaaS service that adds an external dependency, requires a paid plan for production use, and introduces a redirect-based sign-in flow. The baker wants to own their authentication locally, store credentials in their own database, and remove the Clerk dependency entirely.

## Solution

Replace Clerk with ASP.NET Core Identity, a local membership system that stores users, passwords, and roles directly in the app's PostgreSQL database. The single-tenant owner gate currently backed by a custom `AppOwners` table and `OwnerService` will be replaced by ASP.NET Core Identity's built-in role system — the seed user gets an `"Owner"` role, and the middleware checks `User.IsInRole("Owner")`. Authentication remains cookie-based; the OIDC middleware, external sign-in redirect, and `AppOwners` table are all removed. Identity UI is scaffolded as Razor Pages for login, registration, and password management.

## User Stories

1. As the baker, I want to sign in with a username and password stored locally, so that I don't depend on an external identity service.
2. As the baker, I want a login page that looks like the rest of the app, so that the experience is seamless.
3. As the baker, I want to register a new account during initial setup, so that I can create my owner identity.
4. As the baker, I want to change my password after first login, so that I can secure my account with a personal credential.
5. As the baker, I want the first registered user to automatically become the app owner, so that only I have access to the data.
6. As the baker, I want to be redirected to the access-denied page if someone else tries to sign in, so that my data stays private.
7. As the baker, I want to sign out and have my session cleared completely, so that no stale auth state remains.
8. As the baker, I want to see my display name in the header after signing in, so that I know I'm authenticated.
9. As the baker, I want password requirements enforced (uppercase, lowercase, digit, non-alphanumeric, min 6 chars), so that my password has reasonable strength.
10. As the baker, I want email confirmation to be optional, so that I can use a local/fake email without friction.
11. As the baker, I want all pages to require authentication by default, so that no page is accidentally exposed.
12. As the baker, I want the access-denied page to be accessible without authentication, so that denied users can see why they were blocked.
13. As the baker, I want my user data persisted in PostgreSQL, so that it survives app restarts.
14. As the baker, I want the migration to cleanly replace the old `AppOwners` table with Identity tables, so that the schema stays consistent.
15. As the baker, I want the seeded admin account to have fixed credentials documented in the README, so that I know how to log in after deployment.
16. As the baker, I want the app to fail fast at startup if Identity is misconfigured, so that I get a clear error instead of a cryptic runtime failure.

## Implementation Decisions

### 1. Remove Clerk OIDC integration

Remove the following files and components:
- `Auth/ClerkAuthOptions.cs` — Clerk-specific OIDC options record
- `Auth/OpenIdConnectSetup.cs` — OIDC middleware configuration
- `Services/OwnerService.cs` — business logic for admitting/denying by Clerk user ID
- `Auth/OwnerGateMiddleware.cs` — middleware that checks `sub` claim against `AppOwners`

Remove from `Program.cs`:
- `ClerkAuthOptions` configuration binding
- `AddAuthentication().AddOpenIdConnect(...)` chain
- `UseMiddleware<OwnerGateMiddleware>()`
- The fast-fail guard for `Authority`/`ClientId`/`ClientSecret`
- The `/signout` POST endpoint that triggers OIDC end-session

Remove the `OpenIdConnect` NuGet package from the `.csproj`.

### 2. Add ASP.NET Core Identity

Add `Microsoft.AspNetCore.Identity.EntityFrameworkCore` NuGet package.

Create a new `ApplicationDbContext` that extends `IdentityDbContext<IdentityUser>` (or use `IdentityDbContext` with a custom user class if needed). Register it in `Program.cs` with `AddDbContext<ApplicationDbContext>(...)` pointing to the existing PostgreSQL connection string.

Register Identity in `Program.cs`:
```
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
```

Configure Identity options:
- `options.SignIn.RequireConfirmedEmail = false`
- Password options: `RequiredLength = 6`, `RequireDigit = true`, `RequireLowercase = true`, `RequireUppercase = true`, `RequireNonAlphanumeric = true`

Authentication scheme stays cookie-based:
```
builder.Services.AddAuthentication(IdentityConstants.ApplicationScheme)
    .AddIdentityCookies();
```

Authorization fallback policy remains `RequireAuthenticatedUser`.

### 3. Seed user and owner role

Create an `OwnerRoleInitializer` (static method or hosted service) that:
1. Creates the `"Owner"` role if it doesn't exist via `RoleManager<IdentityRole>.CreateAsync`
2. Creates the seed user `admin@ordermanager.local` with password `Admin123!` via `UserManager<IdentityUser>.CreateAsync` if not already present
3. Assigns the `"Owner"` role to the seed user via `UserManager<IdentityUser>.AddToRoleAsync`

Run during app startup in `Program.cs` after `app.Build()`.

### 4. Replace owner gate middleware

Replace `OwnerGateMiddleware` with a new middleware (or simplify the existing one) that checks:
```
context.User.Identity?.IsAuthenticated == true
    && !context.User.IsInRole("Owner")
```
If authenticated but not owner, sign out and redirect to `/access-denied`.

This eliminates the `AppOwners` table, `OwnerService`, and the `ClerkUserId` column entirely.

### 5. Simplify sign-out

`SignOutHandler` simplifies to a single cookie sign-out:
```
await context.SignOutAsync(IdentityConstants.ApplicationScheme);
```
No OIDC end-session redirect needed.

### 6. Database migration

Create a single EF Core migration that:
1. Drops the `AppOwners` table (and its check constraint `CK_AppOwner_SingleRow`)
2. Adds all ASP.NET Core Identity tables (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserRoles`, `AspNetUserTokens`, `AspNetRoleClaims`)

This is a clean-slate migration. No data migration from Clerk subs is needed since the baker is the only user and will re-claim ownership via the seed.

### 7. Scaffold Identity UI

Use `dotnet aspnet-codegenerator identity` to scaffold the Identity Razor Pages into the existing web project. Scaffold into a folder like `Areas/Identity/Pages/` to keep Identity pages separated from the app's Blazor pages.

Update the scaffolded layout to match the app's existing styling (dark theme, Tailwind classes if applicable).

### 8. Update UI claim lookups

`MainLayout.razor` currently extracts `name` or `email` from claims. With ASP.NET Core Identity:
- The `NameIdentifier` claim contains the user GUID (set automatically by Identity)
- The `name` claim maps to `IdentityUser.UserName` or `IdentityUser.Email`
- Update the claim lookup to use `ClaimTypes.Name` or `ClaimTypes.Email` instead of raw `"name"` / `"email"` strings

### 9. Update configuration and documentation

- Remove `Auth:Oidc` section from user-secrets setup in README
- Document the seed user credentials (`admin@ordermanager.local` / `Admin123!`)
- Document the Identity configuration (connection string, password policy)
- Update `CONTEXT.md`: "signed in via Clerk" → "signed in via ASP.NET Core Identity"
- Create new ADR superseding ADR-0001 (Clerk auth) with the Identity decision

## Testing Decisions

### Test seams

The primary test seam is the **owner gate check**. The new middleware's core logic — `User.IsInRole("Owner")` — is the highest seam to test. It can be tested with:
- A `ClaimsPrincipal` with/without the `"Owner"` role claim
- A mock `HttpContext` and `RequestDelegate`
- No database or Identity infrastructure needed for the gate check itself

Secondary seams:
- **Identity configuration** — test that `IdentityOptions` are set correctly (password policy, email confirmation), following the same pattern as `OpenIdConnectSetupTests` (create options, call configure, assert)
- **Role/user seeding** — test `OwnerRoleInitializer` with InMemory DB + `UserManager`/`RoleManager` (following `OwnerServiceTests` pattern with `UseInMemoryDatabase`)
- **Sign-out** — update existing `SignOutHandlerTests` to verify single cookie sign-out instead of dual sign-out

### What to test

- Owner gate allows authenticated users with the `"Owner"` role
- Owner gate denies authenticated users without the `"Owner"` role
- Owner gate allows unauthenticated requests to pass through (handled by fallback policy)
- Identity options bind correctly from configuration
- Seed user is created with correct email and role
- Seed user is not duplicated on second run
- Sign-out clears the authentication cookie

### What not to test

- Scaffolded Identity UI pages (Razor Pages generated by scaffolding — test the behavior via integration tests if needed, not unit tests)
- EF Core Identity table structure (handled by the framework)
- PostgreSQL-specific behavior (covered by InMemory provider in unit tests)

### Prior art

- `OwnerServiceTests.cs` — pattern for InMemory DB + `IDbContextFactory` fake + `IOptions<T>`
- `SignOutHandlerTests.cs` — pattern for `FakeAuthenticationService` and `HttpContext` setup
- `OpenIdConnectSetupTests.cs` — pattern for testing static configuration methods
- `AuthorizationSetupTests.cs` — pattern for testing authorization policy setup

## Out of Scope

- Multi-user support or per-user data scoping (the app remains single-tenant)
- Email confirmation flow (disabled for the seeded user)
- Social login providers (Google, GitHub, etc.)
- Two-factor authentication
- Custom user profile fields beyond Identity defaults
- Integration tests for the full HTTP pipeline (covered by existing manual testing)
- Updating the Dockerfile or docker-compose (no auth-specific changes needed)

## Further Notes

The `AppOwner` model and its migration (`20260827043653_AddAppOwner.cs`) should be left in place as historical migrations — EF Core tracks applied migrations, so removing them would break the migration history. The new migration simply drops the table, which EF Core handles correctly.

The existing `OwnerAuthOptions` (with `AllowClaim`) becomes irrelevant once the role-based gate is in place. It should be removed along with `OwnerService`.
