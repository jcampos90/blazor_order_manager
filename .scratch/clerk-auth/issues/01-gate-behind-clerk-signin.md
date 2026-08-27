# 01: Gate the app behind Clerk sign-in

**What to build:** An unauthenticated visitor to any page of the app is auto-redirected to Clerk-hosted sign-in; after signing in they land back in the app with their identity available to server code. Authentication is wired via the standard ASP.NET Core OpenID Connect middleware (cookie auth, Clerk-hosted pages), with no client-side JS. Clerk dev-instance keys (publishable key, OAuth client ID/secret) live in .NET user-secrets, never committed.

**Blocked by:** None (can start immediately).

**Status:** ready-for-agent

- [ ] An unauthenticated request to any app page redirects to Clerk-hosted sign-in
- [ ] After successful sign-in the user returns to the app and their identity is available server-side (`[Authorize]`, `HttpContext.User`)
- [ ] Auth uses OpenID Connect + cookie middleware; no client-side JS
- [ ] Clerk dev instance + OAuth application exist; publishable key and client ID/secret are in user-secrets (nothing committed)
- [ ] `dotnet build` and `dotnet test` pass

## Comments

Implemented. Wiring: cookie auth (default scheme) + OIDC (default challenge scheme) via
`Auth/OpenIdConnectSetup.cs`; `AuthorizationSetup` sets a fallback policy requiring an
authenticated user, so any unauthenticated request 302s to Clerk's `/oauth/authorize`
(verified live). Identity is exposed via `AddCascadingAuthenticationState()` +
`AuthorizeRouteView`; `MapInboundClaims=false` keeps the `sub` claim for ticket 02.

Manual setup still needed (human step, item 4): create the Clerk OAuth application with
redirect URI `https://localhost:7049/signin-oidc` (and `http://localhost:5195/signin-oidc`),
then from `src/OrderManager.Web` run:

```
dotnet user-secrets set "Auth:Oidc:Authority"    "https://<instance>.clerk.accounts.dev"
dotnet user-secrets set "Auth:Oidc:ClientId"     "<oauth client id>"
dotnet user-secrets set "Auth:Oidc:ClientSecret" "<oauth client secret>"
dotnet user-secrets set "Auth:Oidc:PublishableKey" "<pk_test_...>"   # informational, unused
```

The app fails fast at startup with the above command list until configured.
`dotnet ef` design-time is unaffected.