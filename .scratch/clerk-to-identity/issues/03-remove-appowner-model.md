# 03: Remove AppOwner model

**What to build:** Remove the `AppOwner` entity, its `DbContext` configuration, and all references. The schema becomes clean Identity-only. Historical migrations are left in place (EF Core tracks applied migrations).

**Blocked by:** 02

**Status:** done

- [x] Delete `Models/AppOwner.cs`
- [x] Remove `AppOwners` DbSet and its `OnModelCreating` configuration from `AppDbContext`
- [x] Verify no remaining references to `AppOwner` or `ClerkUserId` in the codebase
- [x] `dotnet build` passes
- [x] `dotnet test` passes
