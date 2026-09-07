# 03: Product delete UX

**What to build:** Soft-delete a Product from `/productos` via a per-row delete icon next to Edit and the IsActive toggle. Clicking opens a confirm modal that shows the count of OrderLines and distinct Orders that reference the Product. Confirming sets `DeletedAt = DateTime.UtcNow`. The IsActive toggle and Delete are independent: a baker can inactivate a seasonal Product without deleting it. Past OrderLines and dashboard rows still resolve the Product to its real name.

**Blocked by:** 01

**Status:** done

- [x] Per-row trash icon button on `/productos` next to Edit and the IsActive toggle; same icon-only ghost style
- [x] Confirm modal: "Eliminar producto" title, body "Vas a eliminar **{Name}**. **{N}** renglones en **{M}** encargos quedan registrados pero {Name} no se ofrecerá en nuevos encargos."
- [x] "N renglones" = `db.OrderLines.Count(l => l.ProductId == id)`; "M encargos" = `db.Orders.Count(o => o.Lines.Any(l => l.ProductId == id))` (or a single grouped query)
- [x] Confirming sets `product.DeletedAt = DateTime.UtcNow`, saves, and reloads the list
- [x] The `/productos` list query filters `Where(p => p.DeletedAt == null)`; the IsActive toggle continues to work independently and toggling IsActive does not set DeletedAt
- [x] OrderEdit's line list and the dashboard still resolve a deleted Product to its real name (verified manually or by test)
- [x] xUnit test (`ProductDeleteTests`): deleting sets `DeletedAt` and excludes the Product from list + new-order dropdown; `OrderLine.UnitPrice` and `OrderLine.ProductId` remain intact; `IsActive` is independent of `DeletedAt`; restore nulls `DeletedAt`
- [x] `dotnet build` and `dotnet test` pass
