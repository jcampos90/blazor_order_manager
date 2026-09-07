# 0004: Staff role, AppUser, and page-level authorization

Amends: 0002-aspnet-core-identity-local-auth (partially)

## Status

Accepted

## Context

ADR-0002 chose stock `IdentityUser` and a blanket `OwnerGateMiddleware` that
signs out anyone without the Owner role. That was correct while the app had a
single sign-in (the seeded admin). The login rework needs the same app to
serve additional sign-ins — staff of the bakery — without giving them
user-management powers, and to let every sign-in change their own password.

Three things have to move at once:

1. The User needs a first-class `DisplayName` (separate from the email used
   to sign in) so the topbar and the user-management table can show a name.
   `IdentityUser` doesn't have one.
2. The system needs a second role (`Staff`) for users who can use the app
   but cannot manage other users. The blanket "must be Owner" middleware
   check is incompatible with that.
3. The Owner needs a page to manage users, which must be reachable from the
   sidebar but only when the signed-in user is the Owner. That is a
   page-level authorization concern, not a middleware one.

The migration from `IdentityUser` to a custom subclass touches every
`Identity*` type registration, the `DbContext`, and the seed initializer.
Weighing the cost of that against the value of a clean `DisplayName`
column, we accept the migration.

## Decision

- **Custom user type.** Introduce `AppUser : IdentityUser` with two new
  properties: `string? DisplayName` (nullable, no default) and
  `bool MustChangePassword` (default `false`). Migrate `AspNetUsers` to add
  both columns. Swap every registration from `IdentityUser` to `AppUser`
  (the `AddIdentity<TUser, TRole>()` call, the `IdentityDbContext` generic
  parameter, `SignInManager<>` / `UserManager<>` generics in services and
  handlers).
- **Second role.** Add `Staff` to the role seeder next to `Owner`. Both
  roles are seeded by `UserRoleInitializer` (formerly
  `OwnerRoleInitializer`) on first run.
- **Middleware scope reduced.** Rename `OwnerGateMiddleware` →
  `AuthGateMiddleware`. The role check is removed. What stays is the
  unauthenticated-passthrough (cookie authentication + `LoginPath`
  redirect in `Program.cs` continue to handle the redirect to `/login`).
- **Page-level authorization.** New `/usuarios` page is decorated with
  `[Authorize(Roles = "Owner")]`. ASP.NET Core's authorization framework
  redirects unauthenticated users to `/login` and authenticated
  non-Owners to `/access-denied` — both paths are wired up by
  `ConfigureApplicationCookie` in `Program.cs`.
- **Sidebar conditional.** The `Usuarios` link in `NavMenu` is wrapped
  in `<AuthorizeView Roles="Owner">` so it doesn't render for Staff.
- **Last-Owner invariant.** `UserService` enforces that no operation
  (role change, delete) can leave the system with zero Owners. Both UI
  (button disabled) and server (thrown `LastOwnerException`) honour it.

## Consequences

**Positive:**
- One Owner, many Staff — the model matches reality without needing a
  multi-tenant data layer.
- Page-level authorization is the standard ASP.NET Core pattern; future
  Owner-only features (e.g. configuration) follow the same template.
- `DisplayName` is a first-class column — queryable, indexable, easy to
  extend later (e.g. locale-aware salutation).

**Negative:**
- One-time migration cost to swap `IdentityUser` for `AppUser`. Every
  test that mocks `UserManager<IdentityUser>` must be updated.
- `AppUser` is a single-table inheritance over `IdentityUser`; future
  role fields added to `AppUser` will keep widening that table.
- Two roles but only one set of UI controls — we don't yet have a way
  to grant *partial* access (e.g. read-only Staff). Adding that later
  means a third role or a permission flag, both of which we'll revisit
  if the bakery actually asks for it.

## Considered Options

- **Keep stock `IdentityUser`; store `DisplayName` in a UserClaim.**
  Rejected. Requires loading claims on every page render and special-casing
  the topbar. A column is simpler.
- **Keep `OwnerGateMiddleware`'s role check; gate `/usuarios` inside the
  page handler.** Rejected. The middleware blanket-rejects Staff before
  any handler runs, so the page-level check is unreachable for Staff.
  Removing the check from middleware is the only way to let Staff sign in.
- **Roles-as-flag on a single `AppUser.Role` enum.** Rejected. ASP.NET
  Core Identity's `UserManager.IsInRoleAsync` and `[Authorize(Roles=…)]`
  are built around the role table; rolling our own means re-implementing
  the attribute machinery.
