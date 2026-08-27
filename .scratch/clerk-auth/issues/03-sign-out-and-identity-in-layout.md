# 03: Sign-out + identity in the layout

**What to build:** The app's header shows the signed-in baker's name or email, plus a **Sign out** button that ends the Clerk session (full end-session) and clears the local auth cookie, returning the visitor to a signed-out state where pages redirect to sign-in.

**Blocked by:** 01 (Gate the app behind Clerk sign-in).

**Status:** ready-for-agent

- [x] The signed-in user's name/email appears in the layout header
- [x] A **Sign out** button ends the Clerk session and clears the local cookie
- [x] After sign-out, app pages redirect to sign-in

## Comments

Implemented. `Auth/SignOutHandler.SignOutAsync` signs out the cookie scheme first (clears the
local auth cookie), then the OIDC scheme (triggers Clerk's remote end-session endpoint). No
manual redirect is issued: the OIDC handler's end-session redirect is the final response, and
Clerk returns the visitor to the app's signed-out callback path where the fallback
authorization policy bounces them to sign-in. Issuing a manual redirect would overwrite the
OIDC end-session redirect and break full end-session. Wired as a `POST /signout` minimal API
with `RequireAuthorization()` (POST, not GET, to avoid prefetch/CSRF-by-link). `MainLayout`
topbar shows the baker's name (else email, else "Usuario") and a "Cerrar sesión" button that
posts the sign-out form with an antiforgery token (`IAntiforgery.GetAndStoreTokens` during
prerender). Tests in `SignOutHandlerTests` cover cookie sign-out, OIDC end-session, the
cookie-before-OIDC ordering (so the end-session redirect is final), and that the handler does
not overwrite the end-session redirect.

Manual setup still needed (human step): the Clerk OAuth application must register the
post-logout redirect URI for the OIDC end-session callback so the visitor returns to the app
after Clerk's logout page. Clerk's discovery exposes `end_session_endpoint`; the ASP.NET OIDC
handler signs out there automatically on OIDC scheme sign-out. Add
`https://localhost:7049/signout-callback-oidc` (and `http://localhost:5195/signout-callback-oidc`)
to the Clerk OAuth app's redirect/authorized URIs.

Status: ready-for-agent (implemented) → implemented