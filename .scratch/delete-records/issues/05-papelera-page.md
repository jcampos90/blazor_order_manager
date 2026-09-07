# 05: Papelera page

**What to build:** A new `/papelera` page lists soft-deleted Customers and Products in two sections. Each row shows the entity name, secondary detail (Phone for Customer; Price + Prep hours for Product), and `DeletedAt` formatted as a date. A "Restaurar" button per row nulls `DeletedAt`, removing the row from the Papelera and making the entity reappear on its main list.

**Blocked by:** 01, 02, 03

**Status:** done

- [x] New page at `/papelera` with two sections: "Clientes eliminados" and "Productos eliminados"
- [x] Customer row: Name, Phone, DeletedAt formatted as a short date, Restaurar button
- [x] Product row: Name, Price (currency), PrepHours, DeletedAt formatted as a short date, Restaurar button
- [x] Each section shows an empty state ("No hay clientes eliminados." / "No hay productos eliminados.") when there are no deleted rows
- [x] "Restaurar" sets `DeletedAt = null` via `db.Customers.Update` / `db.Products.Update`, saves, and refreshes the affected section
- [x] Sidebar (or topbar) nav gains a "Papelera" entry, styled consistently with the existing nav items
- [x] xUnit test (`PapeleraTests`): a soft-deleted Customer and Product both appear in their respective Papelera queries; restoring either nulls `DeletedAt` and removes it from the Papelera query
- [x] `dotnet build` and `dotnet test` pass
