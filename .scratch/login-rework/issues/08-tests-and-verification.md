# 08: Tests + verification

**What to build:** Final pass: every test green, build green, manual smoke.

**Blocked by:** 01–07

**Status:** pending

- [ ] `dotnet build` is clean (no warnings beyond baseline).
- [ ] `dotnet test` is green: existing tests still pass; new `UserServiceTests` and renamed `AuthGateMiddlewareTests` pass.
- [ ] Manual smoke: run `dotnet run --project src/OrderManager.Web`, open `/login`, confirm full-page layout. Sign in as `admin@ordermanager.local` / `Admin123!`. From `/cuenta`, change the password. Sign out, sign in with the new password. From `/usuarios`, create a Staff user, copy the temp password, sign out, sign in as the Staff user, change the password (forced-change banner), confirm `/usuarios` is blocked. Sign back in as Owner, confirm last-Owner guards fire (try to delete self → blocked; try to demote self → blocked).
- [ ] No commits (per global rule). Report build/test results.
