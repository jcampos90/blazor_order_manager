# 01: Add Identity infrastructure

**What to build:** Add ASP.NET Core Identity's database context, NuGet package, service registration, and initial migration so that Identity tables coexist alongside Clerk. The app still authenticates via Clerk — no behavioral change. This lays the foundation for the switchover in ticket 02.

**Blocked by:** None (can start immediately).

**Status:** done

- [x] Add `Microsoft.AspNetCore.Identity.EntityFrameworkCore` NuGet package
- [x] Create `ApplicationDbContext` extending `IdentityDbContext<IdentityUser>`
- [x] Register `ApplicationDbContext` in `Program.cs` using the existing PostgreSQL connection string
- [x] Register Identity services (`AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()`) — do NOT change the authentication scheme yet
- [x] Configure Identity options: `RequireConfirmedEmail = false`, password policy (RequiredLength 6, RequireDigit/Lowercase/Uppercase/NonAlphanumeric all true)
- [x] Create EF Core migration that adds ASP.NET Core Identity tables (does not touch `AppOwners`)
- [x] `dotnet build` passes
- [x] `dotnet test` passes (existing Clerk tests still work)
- [x] App starts and signs in via Clerk as before
