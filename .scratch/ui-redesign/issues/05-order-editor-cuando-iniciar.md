# 05: Order editor: two-column + "Cuándo iniciar"

**What to build:** Creating or editing an encargo shows a two-column layout: the main column holds the customer and the product lines; a sticky sidebar holds delivery date/time, status, total, and a live per-line **"Cuándo iniciar"** summary — each line's start-by time (`PrepSchedule.StartBy(DeliveryAt, product.PrepHours)`) that updates as the delivery time and lines change. The summary is labeled "Cuándo iniciar", never "production plan".

**Blocked by:** 01, 02.

**Status:** ready-for-agent

- [ ] New and edit flows still save an encargo correctly (customer, delivery, status, note, lines, total)
- [ ] Two-column layout with a sticky sidebar showing delivery date/time, status and total
- [ ] "Cuándo iniciar" summary lists each line's start-by time, updating live when the delivery time, a line's product, or a line's quantity changes
- [ ] Validation errors and add/remove line still work; inline styles are replaced by system classes
- [ ] `dotnet build` and `dotnet test` pass