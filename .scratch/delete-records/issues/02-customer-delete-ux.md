# 02: Customer delete UX

**What to build:** Soft-delete a Customer from `/clientes` via a per-row delete icon next to Edit. Clicking opens a confirm modal that shows how many Orders reference the Customer. Confirming sets `DeletedAt = DateTime.UtcNow`; the row disappears from the list. The Orders list, OrderEdit, and dashboard keep resolving the Customer to its real name (no `DeletedAt` filter on the navigation).

**Blocked by:** 01

**Status:** ready-for-agent

- [ ] Per-row trash icon button on `/clientes` next to Edit; same icon-only ghost style as the existing edit button
- [ ] Confirm modal (existing `<Modal>` component) shows "Eliminar cliente" title, body "Vas a eliminar a **{Name}**. Sus **{N}** encargos quedan registrados pero {Name} no estará disponible para nuevos encargos.", Cancelar + Eliminar actions
- [ ] "N encargos" comes from a fresh query against `db.Orders` filtered by `CustomerId` — no `DeletedAt` filter on Order
- [ ] Confirming sets `customer.DeletedAt = DateTime.UtcNow` via `db.Customers.Update`, saves, and reloads the list; cancelling just closes the modal
- [ ] The `/clientes` list query filters `Where(c => c.DeletedAt == null)`; deleted rows no longer render
- [ ] Orders list, OrderEdit, and dashboard still display the deleted Customer's real name (verified manually or by test)
- [ ] xUnit test (`CustomerDeleteTests`): deleting a Customer sets `DeletedAt` and excludes them from the list query; their Orders still resolve to the real name; restore nulls `DeletedAt` and makes them reappear on the list
- [ ] `dotnet build` and `dotnet test` pass
