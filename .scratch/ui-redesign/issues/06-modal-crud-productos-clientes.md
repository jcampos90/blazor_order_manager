# 06: Modal CRUD for Productos & Clientes

**What to build:** Creating and editing products and customers happens in accessible modal dialogs, keeping the list pages uncluttered. One reusable accessible modal pattern (focus enters and traps, Esc closes, overlay click closes, `aria-labelledby`) is used by both pages. The products table gets restyled icon edit actions and the active toggle; the customers table gets restyled icon edit actions.

**Blocked by:** 01, 02.

**Status:** ready-for-agent

- [ ] New/edit product opens in a modal; validation messages show inside; save and close work
- [ ] New/edit customer opens in a modal; save and close work
- [ ] Modal is accessible: focus moves in and is trapped, Esc closes, overlay click closes, `aria-labelledby` is set
- [ ] Products table restyled with icon edit + toggle active; customers table restyled with icon edit
- [ ] `dotnet build` and `dotnet test` pass