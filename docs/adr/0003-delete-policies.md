# 0003: Soft-delete reference data, hard-delete events

## Status

Accepted

## Context

The three domain entities split into two shapes:

- **Reference data**: `Customer`, `Product`. Created once, referenced many times by past and present `Order`s and `OrderLine`s. Their history (orders, prep schedules, totals) outlives their active use.
- **Events**: `Order`. A snapshot of "this customer ordered these products for delivery at this time". Marked `Entregado` and stops being actionable; otherwise it is an intent, not a fact.

The app needs a way to remove records from all three tables without breaking history. Before this decision, every record persisted forever and there was no UI to remove anything.

## Decision

- **`Customer` and `Product` — soft delete.** Add a nullable `DeletedAt` column (`timestamp without time zone`); deleting sets it to `DateTime.UtcNow`. Active lists and the new-order Product dropdown filter `Where(x => x.DeletedAt == null)`. Recovery happens through a dedicated `/papelera` page that nulls `DeletedAt` to restore.
- **`Order` — hard delete.** `db.Orders.Remove(order)` cascades to `OrderLine` via the existing FK (`OnDelete(Cascade)`). Only `Pendiente` and `En producción` Orders are deletable; `Entregado` Orders are locked.
- **Past-Data rule.** Queries that display history (Orders list, OrderEdit lines, dashboard) join through the `Customer` / `Product` navigation without a `DeletedAt` filter, so soft-deleted reference rows still resolve to their real names.

For `Product`, the existing `IsActive` (seasonal / sin stock) and the new `DeletedAt` (remove entirely) stay separate. A Product is offered for new orders iff `IsActive && DeletedAt is null`.

## Consequences

**Positive:**
- Past Orders and OrderLines stay readable even after the referenced Customer or Product is removed.
- Soft-deletes are recoverable from the UI, which is the actual reason to choose soft over hard for reference data.
- Hard-deleting transient Order intents avoids accumulating tombstones with no recovery value.

**Negative:**
- Every new query against `Customer` or `Product` must remember the `DeletedAt IS NULL` filter. Forgotten filters leak deleted rows into the UI.
- Order hard-delete is irreversible without DB backups.

## Considered Options

- **Soft-delete everything**: rejected. Creates tombstones for transient Order intents with no recovery value — once cancelled, an Order is rarely meaningful again.
- **Hard-delete everything**: rejected. Hard-deleting a Customer would cascade-delete their Orders (the Customer navigation is required → default `Cascade`); hard-deleting a Product is blocked outright by `OrderLine.Product`'s `OnDelete(Restrict)`. Either way, history is destroyed.
- **Reuse `IsActive` on Product as the soft-delete flag**: rejected. Collapses two distinct concepts ("off-season" vs "remove entirely") into one column, forcing the baker to choose between "sin stock" and "soft-deleted" semantics.
