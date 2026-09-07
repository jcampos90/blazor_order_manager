# 02: UserService — CRUD with last-Owner guard

**What to build:** New `Services/UserService.cs` (DI-scoped) wrapping `UserManager<AppUser>`. Owns every mutation that touches Users and enforces the `LastOwnerGuard` invariant.

**Blocked by:** 01

**Status:** pending

- [ ] `UserService` is `sealed`, registered as scoped in `Program.cs`.
- [ ] Methods implemented (signatures per spec.md §"Service layer"):
  - `Task<IReadOnlyList<UserSummary>> ListAsync()`
  - `Task<(string TempPassword, string UserId)> CreateAsync(string email, string? displayName, string role)`
  - `Task UpdateAsync(string userId, string? email, string? displayName)`
  - `Task ChangeRoleAsync(string userId, string newRole)`
  - `Task<string> ResetPasswordAsync(string userId)`
  - `Task<bool> DeleteAsync(string userId)` — returns `false` if last-Owner
  - `Task ChangeOwnPasswordAsync(string userId, string currentPassword, string newPassword)`
  - `Task UpdateOwnDisplayNameAsync(string userId, string displayName)`
- [ ] `LastOwnerException` is a public sealed type in `Auth/`. Thrown by `ChangeRoleAsync` and `DeleteAsync` when the action would leave zero Owners.
- [ ] `CreateAsync` generates a 16-char base64-url temp password (no ambiguous chars like `0/O/1/l/I`), creates the user with `MustChangePassword = true`, adds the requested role (`Owner` or `Staff`).
- [ ] `ResetPasswordAsync` removes the existing password, sets a new temp one, flips `MustChangePassword = true`, returns the new temp.
- [ ] `ChangeOwnPasswordAsync` rejects when `currentPassword` doesn't match (re-throws or wraps `PasswordMismatchException`); flips `MustChangePassword = false` on success.
- [ ] `UserSummary` is a public record with `Id`, `Email`, `DisplayName`, `Role`, `CreatedAt`.
- [ ] Tests in `tests/OrderManager.Web.Tests/UserServiceTests.cs` (xUnit + InMemory) cover: create returns temp password, create throws on duplicate email, role change throws LastOwner on last Owner, delete returns false on last Owner, reset password flips `MustChangePassword`, own-password-change rejects wrong current.
- [ ] `dotnet build` + `dotnet test` pass.
