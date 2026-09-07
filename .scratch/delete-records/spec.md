# Delete records — Clientes, Productos, Encargos

**Status:** ready-for-agent

## Problem Statement

The baker has no way to remove a Customer, Product, or Order from the app. Created records stay forever, accumulating noise (a Customer with a typo, a discontinued Product, a test Order). The baker wants delete capability for all three entities without losing the integrity of historical data (past Orders and OrderLines).

## Solution

Three delete flows with different semantics:

- **Customer and Product — soft delete.** Records stay in the DB with a `DeletedAt` timestamp; UI lists and the new-order Product dropdown hide them. A new `/papelera` page surfaces them and lets the baker restore.
- **Order — hard delete.** The row and its `OrderLine`s (FK cascade) are removed. Only `Pendiente` / `En producción` Orders are deletable; `Entregado` is locked.

Each list page (`/clientes`, `/productos`, `/encargos`) gains a per-row delete icon next to the existing Edit. Clicking opens an in-app confirm modal that shows impact detail ("N encargos usan este cliente") before committing.

## User Stories

1. As a baker, I want to delete a Customer I created by mistake, so that the list stays clean.
2. As a baker, I want to delete a discontinued Product, so that it no longer appears in the new-order dropdown.
3. As a baker, I want to delete an Order I created by mistake (only before it's delivered), so that I don't carry a fake record.
4. As a baker, I want to confirm before deleting, with a clear description of what will be affected, so that I don't lose data accidentally.
5. As a baker, I want to restore a Customer or Product I just deleted, so that I can recover from mistakes.
6. As a baker, I want delivered Orders to be undeletable, so that I don't accidentally rewrite delivery history.
7. As a baker, I want past Orders to still show the real customer and product names even after those reference rows are deleted, so that history stays readable.

## Implementation Decisions

### Schema (issue 01)

- Add `DateTime? DeletedAt` to `Customer` and `Product` via an EF Core migration. Default `null`. Column type `timestamp without time zone` (consistent with `Order.DeliveryAt`).
- `Order` schema unchanged. The existing `OrderLine.Order` FK cascade (`OnDelete(Cascade)`) handles hard-delete.
- Convention for future queries: every list/lookup of active Customers or Products filters `Where(x => x.DeletedAt == null)`. Documented inline in `AppDbContext` and in ticket 06.

### Soft-delete UX for Customer (issue 02)

- Per-row delete icon next to Edit on `/clientes`. Same icon-only ghost style as Edit.
- Confirm modal via the existing `<Modal>` component: title "Eliminar cliente", body "Vas a eliminar a **{Name}**. Sus **{N}** encargos quedan registrados pero {Name} no estará disponible para nuevos encargos.", actions Cancelar / Eliminar.
- "N encargos" = `db.Orders.Count(o => o.CustomerId == id)` (no `DeletedAt` filter on `Order`).
- Confirming sets `customer.DeletedAt = DateTime.UtcNow; db.SaveChangesAsync();`. The list query in `Customers.razor` filters `Where(c => c.DeletedAt == null)`.

### Soft-delete UX for Product (issue 03)

- Per-row delete icon next to Edit and the IsActive toggle on `/productos`.
- Confirm modal: "Eliminar producto" title, body "Vas a eliminar **{Name}**. **{N}** renglones en **{M}** encargos quedan registrados pero {Name} no se ofrecerá en nuevos encargos."
- "N renglones" = `db.OrderLines.Count(l => l.ProductId == id)`; "M encargos" = `db.Orders.Count(o => o.Lines.Any(l => l.ProductId == id))` (or a single grouped query).
- `IsActive` toggle and Delete are independent: a baker can inactivate a seasonal Product without deleting it, or delete a discontinued Product outright.
- Confirming sets `product.DeletedAt = DateTime.UtcNow; db.SaveChangesAsync();`.

### Hard-delete UX for Order (issue 04)

- Per-row delete icon next to Edit on `/encargos`. **Only rendered when `o.Status != OrderStatus.Delivered`** — Delivered rows hide it.
- Confirm modal: "Eliminar encargo" title, body "Vas a eliminar el encargo de **{CustomerName}** con entrega **{DeliveryAt:ddd d MMM · HH:mm}**. Sus **{N}** renglones se borran con él."
- "N renglones" = the in-memory `o.Lines.Count`; no extra round-trip needed.
- Confirming calls `db.Orders.Remove(order); db.SaveChangesAsync();`. EF Core cascades to `OrderLine` via the existing FK.

### Papelera page (issue 05)

- New page at `/papelera` with two sections: "Clientes eliminados" and "Productos eliminados".
- Customer row: Name, Phone, DeletedAt formatted as date, Restaurar button.
- Product row: Name, Price (currency), PrepHours, DeletedAt formatted as date, Restaurar button.
- Each section shows an empty state when there are no deleted rows.
- "Restaurar" sets `DeletedAt = null` and refreshes the section.
- Sidebar nav gains a "Papelera" entry, placed consistently with existing items.

### Cross-cutting (issue 06)

- The new-order Product dropdown in `OrderEdit.razor` filters `Where(p => p.IsActive && p.DeletedAt == null)`.
- No auth gate beyond ASP.NET Core Identity (single-user app).
- SPEC.md updated to reflect delete support across all three entities, including the Delivered lock.

## Testing Decisions

xUnit tests under `tests/OrderManager.Web.Tests/`, using the existing InMemory EF `TestFactory` pattern (see `PrepScheduleTests`, `DashboardServiceTests`):

- `CustomerDeleteTests`: soft-delete sets `DeletedAt`; list query excludes deleted Customers; their Orders still resolve to real name.
- `ProductDeleteTests`: soft-delete sets `DeletedAt`; `IsActive` and `DeletedAt` are independent (toggling one doesn't change the other); list and dropdown queries exclude deleted Products; OrderLine rows still resolve the deleted Product's name.
- `OrderDeleteTests`: hard-delete removes the Order and cascades to its OrderLines; status guard prevents deleting `Entregado` Orders; Orders list still resolves deleted Customers' names.
- `PapeleraTests`: a deleted Customer/Product appears in the Papelera query; restore nulls `DeletedAt` and removes it from the Papelera.

`dotnet build` and `dotnet test` must pass before each ticket closes.

## Out of Scope

- Bulk delete.
- A "show deleted" toggle on the main lists — the Papelera is the only entry point.
- Audit trail beyond `DeletedAt` (no "deleted by X" — single-user app).
- Soft-delete for Orders.
- Re-parenting Orders from a deleted Customer to another Customer.
- Cascading soft-delete (deleting a Customer does not touch their Orders).

## Further Notes

- ADR-0003 captures the asymmetry: soft-delete for reference data (`Customer`, `Product`), hard-delete for events (`Order`).
- All UI labels follow the `CONTEXT.md` glossary: "cliente", "producto", "encargo", "renglón", "entrega". The Papelera page is labeled "Papelera" (household term in AR Spanish).
- The `Customer.Customer` navigation in `AppDbContext` is `IsRequired()`; since we soft-delete, the past-data rule (no `DeletedAt` filter on history joins) means Orders keep showing the Customer's real name without violating the required-navigation constraint.
