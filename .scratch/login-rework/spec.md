# Login Rework — Full-page login, password change, user CRUD

**Status:** ready-for-agent

## Problem Statement

The login page renders inside `MainLayout`, so the sidebar and topbar are visible
on `/login`. The default admin (`admin@ordermanager.local`) can sign in but has
no way to change its own password, and no way to provision additional people
(staff of the bakery) who can also use the app. The current model treats every
sign-in as the Owner — `OwnerGateMiddleware` actively signs out anyone who
isn't — so the model needs to widen as well as the UI.

## Solution

Three coordinated changes:

1. **Full-page login.** New `LoginLayout` with no sidebar, no topbar, no
   navigation — just the brand mark and the form. `Login.razor` and
   `AccessDenied.razor` switch to it. The login form is polished: clearer
   errors, branded panel, focus management, "Recordame" checkbox left in
   (already the default via `isPersistent: true`).
2. **Self-service password change.** New `/cuenta` page (UI: "Mi cuenta")
   reachable from the topbar identity area. Available to both `Owner` and
   `Staff`. Lets the signed-in user change their own password (current + new)
   and edit their own display name. Email and role are read-only here.
3. **Owner-only user CRUD.** New `/usuarios` page (UI: "Usuarios")
   reachable from the sidebar (only visible to Owners). Lists users with
   email, display name, role, and created date. Owner can create users
   (system generates a temp password shown once), edit email / display name /
   role, reset any user's password (returns a new temp password shown once),
   and hard-delete users. Last-Owner rules block demote / delete / role
   change that would leave the system with zero Owners.

`OwnerGateMiddleware` keeps its unauthenticated redirect (its other job) but
drops the blanket role check. The role check moves to the page level via
`[Authorize(Roles = "Owner")]` on `/usuarios`. `MainLayout` / `Login` /
`AccessDenied` continue to use the existing cookie-based authentication
state.

`IdentityUser` is replaced by an `AppUser : IdentityUser { DisplayName }`
so the display name has a first-class column. Identity's roles table
gains a second role `Staff` seeded alongside `Owner`. The seed admin
(`admin@ordermanager.local` / `Admin123!`) stays; Owner is also able to
change that password from `/cuenta`.

## Domain Model (locked)

- **User** — anyone who can sign in to the app, identified by email.
  Carries a `DisplayName` for the UI. Two roles: `Owner` and `Staff`.
- **Owner** — a User with the Owner role. Manages users and runs the bakery.
  In practice there is one Owner (the seed admin), but the system supports
  more than one.
- **Staff** — a User with the Staff role. Can use every page except
  `/usuarios`. Can change own password and display name from `/cuenta`.
- **DisplayName** — a User's human-readable name (e.g. "María"). Separate
  from the email used to sign in. Optional at create time; the email is
  the fallback everywhere.
- **TempPassword** — a randomly generated initial / reset password, shown
  to the Owner exactly once after creation / reset, never persisted
  readable. The new user must change it on first sign-in (enforced via a
  `MustChangePassword` flag on `AppUser`).
- **LastOwnerGuard** — invariant: the system always has at least one User
  with the Owner role. Enforced on role change, disable, and hard-delete.
- **Single tenant** — re-defined: the app holds one bakery's data. Auth
  gates access; it never scopes data per user. (See `CONTEXT.md` update.)

## User Stories

1. As the baker, when I open `/login` I want a clean page focused on signing
   in, with no sidebar, so I don't feel like the app is half-loaded.
2. As the baker, I want to change my own password from a `Mi cuenta` link
   in the topbar, so I don't have to rely on hardcoded seed credentials.
3. As the baker, I want to invite a helper to use the app, so they can take
   orders on my behalf when I'm not at the counter.
4. As the baker, when I create a user I want a one-time temp password I can
   hand to the new person, so they can sign in immediately without me
   needing to send an email.
5. As the baker, I want the new user to be forced to choose their own
   password on first sign-in, so a password I shared verbally doesn't stay
   valid forever.
6. As the baker, I want to change any user's password, edit their email /
   display name, and change their role, so I can correct mistakes without
   having to delete and re-create the account.
7. As the baker, I want to remove a user who no longer works at the
   bakery, so the sign-in list stays current.
8. As the baker, I want the system to stop me from removing the last
   Owner (or downgrading them to Staff), so I never lock myself out.
9. As a staff member, I want to change my own password and display name
   from `Mi cuenta`, so my sign-in feels mine without me having to ask
   the Owner for every change.
10. As a staff member, I want to be blocked from the `Usuarios` page
    silently, so I don't see an empty admin UI when I can't use it.

## Implementation Decisions

### Domain & schema (issue 01)

- New `AppUser : IdentityUser` with `string? DisplayName` and `bool MustChangePassword` (default `false`). The latter flips to `true` when the Owner creates or resets a user.
- `ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole, string>` — the parameterised generic is needed because `IdentityDbContext` defaults to `IdentityUser`; switch to the explicit form.
- New EF Core migration: `AddAppUser`. Adds `DisplayName` and `MustChangePassword` columns to `AspNetUsers`. The migration also seeds the `Staff` role (next to `Owner`) via `modelBuilder.Entity<IdentityRole>().HasData(...)`. The seed admin user is created by the existing `OwnerRoleInitializer` and gets `DisplayName = "Admin"` and `MustChangePassword = false`.
- `OwnerRoleInitializer` is renamed to `UserRoleInitializer` and grows a new responsibility: create the `Staff` role if missing.

### Service layer (issue 02)

- New `UserService` (DI-scoped) wraps `UserManager<AppUser>` and exposes:
  - `Task<IReadOnlyList<UserSummary>> ListAsync()`
  - `Task<(string tempPassword, string userId)> CreateAsync(string email, string? displayName, string role)`
  - `Task UpdateAsync(string userId, string? email, string? displayName)` (role change has its own method)
  - `Task ChangeRoleAsync(string userId, string newRole)`
  - `Task<string> ResetPasswordAsync(string userId)`
  - `Task<bool> DeleteAsync(string userId)` — returns `false` if the user is the last Owner (UI uses this)
  - `Task ChangeOwnPasswordAsync(string userId, string currentPassword, string newPassword)`
  - `Task UpdateOwnDisplayNameAsync(string userId, string displayName)`
- Each method that mutates role, removes a user, or deletes a user calls a private `EnsureNotLastOwnerAsync(targetUserId, roleAfter)` guard that throws `LastOwnerException` if the action would leave zero Owners.
- `CreateAsync` validates the email isn't already taken, generates a 16-char base64-url temp password (no ambiguous chars), creates the user with `MustChangePassword = true`, adds the requested role (defaults to `Staff`), and returns the temp password for the Owner to communicate.
- `ResetPasswordAsync` is the admin escape hatch: it removes the existing password, sets a new temp one, flips `MustChangePassword = true`, and returns the new temp.

### Auth gate (issue 03)

- `OwnerGateMiddleware` becomes `AuthGateMiddleware`. The role check is dropped. What stays: the request is allowed through if the user is unauthenticated (the cookie scheme + `LoginPath` redirect in `Program.cs` handle the redirect to `/login`; no change to that flow). The file is renamed; the existing test class is renamed accordingly and its tests updated to assert only that anonymous traffic flows through.
- `[Authorize(Roles = "Owner")]` is added to `Pages/Usuarios.razor`. The component renders an empty shell with a "No tenés permisos" message if `AuthenticationState` somehow shows a non-Owner — defence in depth, since the attribute already short-circuits.
- The seeded admin's role (Owner) is unchanged. Existing tests that mock an authenticated non-Owner hitting the middleware continue to pass through.

### Login layout (issue 04)

- New `Components/Layout/LoginLayout.razor`. Renders a single centered panel: brand mark (same SVG as the sidebar) + the page body. No `<aside>`, no `<header>`. Includes a hidden `<ReconnectModal />` so the WebSocket still works.
- `Login.razor` switches its `@layout` directive from `MainLayout` to `LoginLayout`. The form gains:
  - Inline error band with friendly Spanish copy (already has `error=invalid` / `error=missing`; add `error=expired` for sessions that were kicked by the gate).
  - "Iniciar sesión" submit button styled the same as the primary button on the main app.
  - The card max-width stays 380px; the page uses `min-height:100vh` and a soft gradient background to feel like a full page, not a popup.
- `AccessDenied.razor` also switches to `LoginLayout`. The copy stays short: "No tenés permisos para entrar a esta página." with a `Volver al inicio` link.

### Self-service `/cuenta` page (issue 05)

- Route `/cuenta`. `[Authorize]` (any signed-in user). Uses `MainLayout`.
- Two stacked sections in a single card:
  1. **Datos** — `DisplayName` input (read-only email). Save button calls `UserService.UpdateOwnDisplayNameAsync`.
  2. **Contraseña** — current password, new password, confirm new password. Save button calls `UserService.ChangeOwnPasswordAsync`. Errors from Identity (e.g. `PasswordMismatch`, weak new password) surface inline.
- The page also exposes a "Tenés que cambiar tu contraseña" banner when `MustChangePassword = true`, with the password-change section above the fold. The banner is the only thing visible until they change it; everything else on the page is hidden behind a disabled state. After a successful change, the banner disappears and `MustChangePassword` flips to `false`.

### Owner-only `/usuarios` page (issue 06)

- Route `/usuarios`. `[Authorize(Roles = "Owner")]`. Uses `MainLayout`. Includes the same `<Modal>` component pattern used by the delete-records feature.
- Table columns: `DisplayName` (or email fallback), email, role (Owner / Staff), creado (date), acciones (`Editar`, `Cambiar contraseña`, `Eliminar`).
- `Crear usuario` button opens a modal: email (required), display name (optional), role (Staff by default; Owner available but flagged "Cuidado"). Submit returns the temp password in a follow-up modal with a copy button and an OK button (clears the state).
- `Editar` opens a modal: email, display name, role. Save calls `UpdateAsync` + `ChangeRoleAsync` if the role changed. If the role-change would leave zero Owners, the modal shows the error and the dropdown reverts.
- `Cambiar contraseña` is a one-click action that calls `ResetPasswordAsync` and shows the new temp password in the same one-shot modal as create.
- `Eliminar` opens a confirm modal that names the user and warns "Vas a eliminar a **{DisplayName}**. Ya no podrá iniciar sesión." Submit calls `DeleteAsync`. If the user is the last Owner, the modal is blocked at render time and the row's delete button is disabled with a tooltip "No podés eliminar al único Owner".

### Sidebar (issue 07)

- `NavMenu.razor` gains a new section at the bottom of `.nav` (above `.sidebar-foot`):
  ```
  <AuthorizeView Roles="Owner">
    <Authorized>
      <NavLink href="usuarios">Usuarios</NavLink>
    </Authorized>
  </AuthorizeView>
  ```
- `MainLayout.razor`'s topbar gains a `Mi cuenta` button next to the identity label, before `Cerrar sesión`. Both buttons go through a small form-post pattern consistent with sign-out.

### Tests (issue 08)

- New `UserServiceTests` using the existing xUnit + EF Core InMemory setup (same as `CustomerServiceTests`):
  - `CreateAsync_returns_temp_password_and_creates_user_with_Staff_role`
  - `CreateAsync_throws_when_email_already_taken`
  - `ChangeRoleAsync_throws_LastOwnerException_when_demoting_last_owner`
  - `DeleteAsync_returns_false_when_target_is_last_owner`
  - `ResetPasswordAsync_marks_user_with_MustChangePassword`
  - `ChangeOwnPasswordAsync_rejects_wrong_current_password`
- `OwnerGateMiddlewareTests` is renamed to `AuthGateMiddlewareTests`; the non-Owner case is removed (it now passes through). One test asserts an anonymous request is allowed through (the redirect is the cookie scheme's job). One test asserts an authenticated Owner is allowed through.
- Existing tests that asserted middleware rejection of non-Owners are updated or removed (TBD per file).
- A new `LoginLayoutTests` smoke test isn't needed (the layout is markup); the existing `Login.razor` no longer references `MainLayout` so visual regression is observable in the running app.

## File Touch List (rough)

- `src/OrderManager.Web/Models/AppUser.cs` — new
- `src/OrderManager.Web/Data/ApplicationDbContext.cs` — generic param swap
- `src/OrderManager.Web/Auth/OwnerRoleInitializer.cs` → `Auth/UserRoleInitializer.cs` — rename + Staff role
- `src/OrderManager.Web/Auth/OwnerGateMiddleware.cs` → `Auth/AuthGateMiddleware.cs` — rename + drop role check
- `src/OrderManager.Web/Auth/AuthorizationSetup.cs` — unchanged
- `src/OrderManager.Web/Services/UserService.cs` — new
- `src/OrderManager.Web/Program.cs` — swap `IdentityUser` → `AppUser`, rename DI registrations
- `src/OrderManager.Web/Migrations/<timestamp>_AddAppUser.cs` — generated
- `src/OrderManager.Web/Components/Layout/LoginLayout.razor` — new
- `src/OrderManager.Web/Components/Pages/Login.razor` — switch layout
- `src/OrderManager.Web/Components/Pages/AccessDenied.razor` — switch layout
- `src/OrderManager.Web/Components/Pages/Cuenta.razor` — new
- `src/OrderManager.Web/Components/Pages/Usuarios.razor` — new
- `src/OrderManager.Web/Components/Layout/MainLayout.razor` — topbar `Mi cuenta` link
- `src/OrderManager.Web/Components/Layout/NavMenu.razor` — sidebar `Usuarios` link
- `tests/OrderManager.Web.Tests/UserServiceTests.cs` — new
- `tests/OrderManager.Web.Tests/OwnerGateMiddlewareTests.cs` → `AuthGateMiddlewareTests.cs` — rename + update
- `CONTEXT.md` — sharpen `User`, add `Owner`, `Staff`, `DisplayName`, `TempPassword`, `LastOwnerGuard`; rewrite `Single tenant`
- `docs/adr/0004-staff-role-and-user-management.md` — new

## Acceptance

- [ ] `dotnet build` and `dotnet test` pass.
- [ ] `/login` shows a full-page layout (no sidebar, no topbar).
- [ ] The seed admin can change its own password from `/cuenta`.
- [ ] The seed admin can create a `Staff` user from `/usuarios`, sees a temp password once, and the new user is forced to change it on first sign-in.
- [ ] The seed admin can edit / reset-password / delete the new user.
- [ ] The seed admin cannot demote, disable, or delete itself if it's the only Owner (UI button disabled + server-side guard).
- [ ] A `Staff` user can change their own password and display name but not their email, role, or any other user.
- [ ] A `Staff` user is redirected (or shown an empty state) when hitting `/usuarios`.
- [ ] `CONTEXT.md` reflects the sharpened terms.
- [ ] An ADR explains why `IdentityUser` was widened to `AppUser` and why the gate moved from middleware to page-level.
