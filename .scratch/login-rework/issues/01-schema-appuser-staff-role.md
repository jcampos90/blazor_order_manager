# 01: Schema — AppUser with DisplayName and MustChangePassword; Staff role seeded

**What to build:** Replace stock `IdentityUser` with `AppUser : IdentityUser { string? DisplayName, bool MustChangePassword }`; swap the `IdentityDbContext` and `AddIdentity` generics accordingly; generate an EF Core migration; seed the `Staff` role alongside `Owner`.

**Blocked by:** None

**Status:** pending

- [ ] `Models/AppUser.cs` exists with `DisplayName` (nullable) and `MustChangePassword` (default `false`).
- [ ] `ApplicationDbContext` is declared as `IdentityDbContext<AppUser, IdentityRole, string>` (or the equivalent generic that matches `AddIdentity<TUser, TRole>`).
- [ ] `Program.cs` registers `AddIdentity<AppUser, IdentityRole>()` instead of `AddIdentity<IdentityUser, IdentityRole>()`.
- [ ] `Auth/OwnerRoleInitializer.cs` is renamed to `Auth/UserRoleInitializer.cs` and grows a `Staff` role seed. Seed admin user gets `DisplayName = "Admin"`, `MustChangePassword = false`.
- [ ] `SignInManager<AppUser>` and `UserManager<AppUser>` are updated in `SignInHandler.cs` and any other consumer.
- [ ] EF migration `AddAppUser` is generated and applies cleanly (`dotnet ef database update`).
- [ ] `dotnet build` + `dotnet test` pass after the swap. Existing test files that mock `UserManager<IdentityUser>` are updated to mock `UserManager<AppUser>` (if any).
