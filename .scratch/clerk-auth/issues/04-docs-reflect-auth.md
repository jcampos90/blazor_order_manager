# 04: Docs reflect auth

**What to build:** The project docs no longer describe the app as unauthenticated. The spec drops the "sin autenticación" assumption and moves authentication out of the "ask first" boundaries; the README gains an Auth section covering sign-in, the first-sign-in ownership claim, and how to configure the dev keys.

**Blocked by:** 02 (First sign-in claims ownership).

**Status:** ready-for-agent

- [x] The spec no longer says the app is unauthenticated; auth is removed from the "ask first" boundaries
- [x] The README has an Auth section: sign-in, first-sign-in ownership claim, configuring dev keys
- [x] No stale claims about authentication remain

## Comments

Implemented. `SPEC.md` drops the "Sin autenticación" tech-stack line (now lists Clerk OIDC,
single-tenant, first-sign-in claim, referencing ADR-0001), removes `auth/` from the "ask
first" boundaries (keeps `multiusuario` = per-user data scoping), and rewords assumption 5
("single-user local" → "un solo propietario vía Clerk"). The `Run` command and the first
success criterion now note the Clerk user-secrets prerequisite (the app fast-fails without
them). `README.md` gains an `Autenticación` section covering Clerk-hosted sign-in, the
first-sign-in ownership claim (`Auth:AllowClaim`, `AppOwners` single-row table), sign-out
(remote end-session + cookie clear), and the dev-key setup (user-secrets key names +
redirect URIs for ports 7049/5195). Also fixed the stale launch URL (`7002` → `7049`/`5195`)
and added Clerk to Requisitos/Stack.

Code review feedback incorporated: `AllowClaim` "por defecto en desarrollo" was inaccurate
(it defaults on in all environments; disabling for production is a manual step) and is now
stated correctly. No code changes — docs only. `dotnet build` and `dotnet test` (28) pass.

Status: ready-for-agent → implemented