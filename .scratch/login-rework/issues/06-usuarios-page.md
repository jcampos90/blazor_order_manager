# 06: `/usuarios` — Owner-only user CRUD with temp-password flow

**What to build:** New `Pages/Usuarios.razor` (UI: "Usuarios"), `[Authorize(Roles = "Owner")]`. Lists users; create / edit / reset-password / delete via modals. Honours `LastOwnerGuard` in both UI (button disabled) and server.

**Blocked by:** 01, 02

**Status:** pending

- [ ] Route `/usuarios`, `[Authorize(Roles = "Owner")]`, uses `MainLayout`.
- [ ] Table columns: `DisplayName` (or email fallback), email, role (Owner / Staff), creado (date), acciones.
- [ ] **Crear usuario modal:** email (required), display name (optional), role (Staff default; Owner available with a "Cuidado" hint). On submit, the result modal shows the temp password with a copy button. The temp-password modal can only be dismissed; closing it doesn't keep it around.
- [ ] **Editar modal:** email, display name, role. If the role change would violate the last-Owner guard, the dropdown reverts and an inline error appears.
- [ ] **Cambiar contraseña action:** calls `UserService.ResetPasswordAsync`; result modal shows the new temp password with a copy button.
- [ ] **Eliminar confirm modal:** names the user, warns "Ya no podrá iniciar sesión." Submit calls `UserService.DeleteAsync`. If the user is the last Owner, the row's delete button is disabled with tooltip "No podés eliminar al único Owner".
- [ ] Reusable modal markup lives in `Components/Shared/Modal.razor` if not already extracted (check the delete-records feature first; reuse if present).
- [ ] `dotnet build` passes.
