# 02: Switch auth to Identity + owner gate

**What to build:** The full switchover from Clerk to ASP.NET Core Identity. The baker signs in with username/password, is admitted as Owner via a role, and non-owners are denied. All Clerk components are removed. This is the single atomic change that makes Identity the active auth system.

**Blocked by:** 01

**Status:** done

- [x] Create new `OwnerGateMiddleware` that checks `User.IsInRole("Owner")` — authenticated non-owners are signed out and redirected to `/access-denied`
- [x] Create `OwnerRoleInitializer` that seeds the `"Owner"` role and the admin user (`admin@ordermanager.local` / `Admin123!`) with the role assigned
- [x] Switch authentication scheme from OIDC to `IdentityConstants.ApplicationScheme` with `AddIdentityCookies()`
- [x] Update `Program.cs`: replace `UseMiddleware<OwnerGateMiddleware>()` with the new Identity-based gate, run `OwnerRoleInitializer` at startup, remove OIDC endpoint (`/signout` POST becomes Identity sign-out)
- [x] Remove `Auth/ClerkAuthOptions.cs`, `Auth/OpenIdConnectSetup.cs`, `Auth/OwnerGateMiddleware.cs` (old), `Services/OwnerService.cs`, `Auth/OwnerAuthOptions.cs`
- [x] Remove `Auth:Oidc` configuration binding and fast-fail guard from `Program.cs`
- [x] Remove `Microsoft.AspNetCore.Authentication.OpenIdConnect` NuGet package
- [x] Remove Clerk-related tests: `ClerkAuthOptionsTests.cs`, `OpenIdConnectSetupTests.cs`, `OwnerAuthOptionsTests.cs`, `OwnerServiceTests.cs`
- [x] Add new tests: `OwnerGateMiddlewareTests` (allows Owner, denies non-Owner, passes unauthenticated), `OwnerRoleInitializerTests` (seeds role and user, idempotent)
- [x] `dotnet build` passes
- [x] `dotnet test` passes
- [x] App starts, seed user is created, sign-in works, owner gate blocks non-Owner users
