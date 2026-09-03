# 0002: ASP.NET Core Identity with local cookie auth

Supersedes: 0001-clerk-auth-oidc-single-tenant

## Status

Accepted

## Context

The app previously used Clerk as an external OpenID Connect provider for authentication.
While Clerk provided a hosted sign-in experience, it introduced an external dependency for
a single-tenant bakery tool that runs locally. The app needs only one authenticated user —
the baker — with no multi-tenancy or external user management requirements.

ASP.NET Core Identity with cookie authentication provides a self-contained solution that:
- Requires no external service accounts or API keys
- Runs entirely within the app's process
- Integrates natively with Blazor Server's `AuthenticationStateProvider`
- Supports role-based authorization for the owner gate pattern

## Decision

Replace Clerk OIDC authentication with ASP.NET Core Identity using:

- **Stock `IdentityUser` / `IdentityRole` types** — no custom user classes needed
- **Cookie authentication** via `.AddIdentityCookies()` — no external providers
- **Dual DbContext design** — `ApplicationDbContext` (Identity) and `AppDbContext` (domain)
  share the same PostgreSQL database but keep schemas isolated
- **Role-based owner gate** — `OwnerGateMiddleware` checks for the `Owner` role after
  authentication; users without the role are signed out and redirected to `/access-denied`
- **Seed data** — `OwnerRoleInitializer` creates the `Owner` role and an admin user
  (`admin@ordermanager.local` / `Admin123!`) on first run
- **Password policy** — minimum length 6, requires digit, lowercase, uppercase, and
  non-alphanumeric characters; confirmed email not required

## Consequences

**Positive:**
- No external dependency on Clerk — app runs fully offline / locally
- Simpler deployment — no OAuth application registration or redirect URI configuration
- Native ASP.NET Core integration — standard middleware pipeline, no JS required
- Seed credentials enable immediate development without setup steps

**Negative:**
- No hosted sign-in UI — login page is the default Identity scaffold (functional but basic)
- Seed credentials are hardcoded — must be changed or removed for production
- No external identity verification — relies solely on local password storage

## Considered Options

- **Keep Clerk OIDC**: rejected. External dependency adds setup friction for a local-only tool;
  Clerk's free tier limits are unnecessary for single-user apps.
- **Custom authentication middleware**: rejected. Reimplementing password hashing, session
  management, and cookie handling is error-prone when Identity provides all of this.
- **Third-party local auth library**: rejected. Identity is built into ASP.NET Core and
  sufficient for this use case.
