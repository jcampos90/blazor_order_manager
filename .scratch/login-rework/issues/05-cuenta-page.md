# 05: `/cuenta` — self-service display name + password change

**What to build:** New `Pages/Cuenta.razor` reachable from the topbar. Lets the signed-in user change their own display name and password. Shows a forced-change banner when `MustChangePassword = true`.

**Blocked by:** 01, 02

**Status:** pending

- [ ] Route `/cuenta`, attribute `[Authorize]` (any signed-in user), uses `MainLayout`.
- [ ] Loads the current `AppUser` via `UserManager.GetUserAsync(User)` (or `UserManager.FindByIdAsync` from the `AuthenticationState`).
- [ ] **Datos section:** read-only email, editable `DisplayName`. Save calls `UserService.UpdateOwnDisplayNameAsync`. Success toast / inline confirmation.
- [ ] **Contraseña section:** current password, new password, confirm new password. Save calls `UserService.ChangeOwnPasswordAsync`. Errors (`PasswordMismatch`, weak new password per Identity policy) surface inline with friendly Spanish copy.
- [ ] **Forced-change banner:** when `MustChangePassword = true`, only the `Contraseña` section is interactive; the `Datos` section is hidden behind a disabled state. Banner text: "Antes de seguir tenés que cambiar tu contraseña." After successful change, banner disappears and the page unlocks.
- [ ] On every render, if the user has signed out mid-session, redirect to `/login`.
- [ ] `dotnet build` passes.
