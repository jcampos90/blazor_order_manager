# 04: Order delete UX

**What to build:** Hard-delete an Order from `/encargos` via a per-row delete icon next to Edit. The icon only renders for `Pendiente` and `En producción` Orders; `Entregado` Orders hide it. Confirming removes the Order and cascades to its `OrderLine`s via the existing FK.

**Blocked by:** None

**Status:** ready-for-agent

- [ ] Per-row trash icon button on `/encargos`, only rendered when `o.Status != OrderStatus.Delivered`
- [ ] Confirm modal: "Eliminar encargo" title, body "Vas a eliminar el encargo de **{CustomerName}** con entrega **{DeliveryAt:ddd d MMM · HH:mm}**. Sus **{N}** renglones se borran con él."
- [ ] "N renglones" = the in-memory `o.Lines.Count`; no extra round-trip needed
- [ ] Confirming calls `db.Orders.Remove(order)` followed by `db.SaveChangesAsync()`; EF Core cascades to OrderLine via the existing FK (`OnDelete(Cascade)`)
- [ ] After delete, the list reloads and the row is gone
- [ ] xUnit test (`OrderDeleteTests`): hard-deleting a `Pendiente` Order removes its OrderLines; the status guard prevents deleting an `Entregado` Order (helper that asserts the guard); the Orders list query still resolves deleted Customers' real names
- [ ] `dotnet build` and `dotnet test` pass
