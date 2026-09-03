# 04: Update UI claim lookups

**What to build:** Update `MainLayout.razor` to extract the baker's display name using ASP.NET Core Identity claim types instead of Clerk's raw `"name"` / `"email"` strings.

**Blocked by:** 02

**Status:** resolved

- [x] Update `MainLayout.razor` claim lookup to use `ClaimTypes.Name` and `ClaimTypes.Email` instead of raw `"name"` and `"email"` strings
- [x] Verify the header displays the signed-in user's name correctly
- [x] `dotnet build` passes
- [x] `dotnet test` passes
