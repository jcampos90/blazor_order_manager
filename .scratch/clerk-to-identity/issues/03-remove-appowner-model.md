# 03: Remove AppOwner model

**What to build:** Remove the `AppOwner` entity, its `DbContext` configuration, and all references. The schema becomes clean Identity-only. Historical migrations are left in place (EF Core tracks applied migrations).

**Blocked by:** 02

**Status:** ready-for-agent

- [ ] Delete `Models/AppOwner.cs`
- [ ] Remove `AppOwners` DbSet and its `OnModelCreating` configuration from `AppDbContext`
- [ ] Verify no remaining references to `AppOwner` or `ClerkUserId` in the codebase
- [ ] `dotnet build` passes
- [ ] `dotnet test` passes
