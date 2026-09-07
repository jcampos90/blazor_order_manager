# 03: AuthGateMiddleware — drop the blanket Owner check

**What to build:** Rename `OwnerGateMiddleware` to `AuthGateMiddleware`. The role check is removed; the unauthenticated passthrough stays (cookie auth + `LoginPath` already handle redirects). Update existing tests accordingly.

**Blocked by:** None (independent of the AppUser swap; can land in parallel)

**Status:** pending

- [ ] `Auth/OwnerGateMiddleware.cs` is renamed to `Auth/AuthGateMiddleware.cs`.
- [ ] The middleware body contains no role check. The class still exposes `InvokeAsync` so the pipeline shape doesn't change.
- [ ] `Program.cs` swaps `app.UseMiddleware<OwnerGateMiddleware>()` for `app.UseMiddleware<AuthGateMiddleware>()`.
- [ ] `tests/OrderManager.Web.Tests/OwnerGateMiddlewareTests.cs` is renamed to `AuthGateMiddlewareTests.cs`. The "non-Owner is signed out" test is deleted. Two tests remain: anonymous request flows through, authenticated Owner flows through.
- [ ] `dotnet build` + `dotnet test` pass.
