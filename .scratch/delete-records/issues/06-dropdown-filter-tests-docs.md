# 06: Cross-cutting — dropdown filter, tests, docs

**What to build:** Wire the new-order Product dropdown to exclude soft-deleted Products, sanity-sweep the rest of the app for any list or lookup still surfacing deleted rows, and update SPEC.md to mark delete as supported across all three entities.

**Blocked by:** 01, 02, 03, 04, 05

**Status:** done

- [x] `OrderEdit.razor`'s Product dropdown query filters `Where(p => p.IsActive && p.DeletedAt == null)` so soft-deleted Products never appear as choices for new OrderLines
- [x] Sanity sweep across the codebase: every `db.Customers` and `db.Products` query has the right `DeletedAt` filter for its purpose — list/dropdown queries exclude deleted rows; history joins (Orders list, OrderEdit, dashboard) intentionally do not
- [x] All existing tests pass; the new tests added in 02 / 03 / 04 / 05 are present and passing
- [x] `SPEC.md` updated: an Open Question or section notes that delete is now supported for Clientes, Productos, and Encargos (the latter with the `Entregado` lock), and the `Customer` / `Product` table rows mention soft-delete + Papelera recovery
- [x] `dotnet build` and `dotnet test` pass
