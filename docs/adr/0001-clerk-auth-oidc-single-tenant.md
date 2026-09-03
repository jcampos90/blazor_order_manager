# Clerk auth via OpenID Connect, single-tenant gate

**Status:** Superseded by [0002-aspnet-core-identity-local-auth.md](0002-aspnet-core-identity-local-auth.md)

The app was single-user and unauthenticated (personal use). We added Clerk as the identity
provider, integrated through the standard ASP.NET Core OpenID Connect middleware — cookie auth
with Clerk-hosted sign-in — instead of the `@clerk/clerk-js` SDK, because Blazor Server's
circuit-based `[Authorize]`/`AuthenticationStateProvider` model maps natively onto OIDC + cookies
and needs no client-side JS. Access is gated to a single owner: the first successful sign-in
claims ownership while `Auth:AllowClaim` is on, and only that Clerk user ID (`sub` claim) is
admitted thereafter. Data is not scoped per user; moving to per-user scoping would be a schema
and query change, not an auth change.

## Considered Options

- **`@clerk/clerk-js` + backend JWT verification**: rejected. Clerk ships no official ASP.NET
  middleware, and the JS-SDK path fights Blazor Server's server-rendered auth state instead of
  integrating with it.
- **Multi-user scoping**: rejected for now. The app is a single bakery; `OwnerId` on entities and
  per-user query filters were judged premature.