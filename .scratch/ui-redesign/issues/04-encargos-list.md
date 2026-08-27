# 04: Encargos list

**What to build:** The encargos list uses clean rows with icon-only ghost edit actions and a labeled "Marcar entregado" success button, so scanning and acting on encargos stays fast. The filters (Todos · Hoy · Mañana · date) are restyled as chips.

**Blocked by:** 01, 02.

**Status:** ready-for-agent

- [ ] Table rows render in the new style; delivered rows are muted
- [ ] Edit is an icon-only ghost action; "Marcar entregado" is a labeled success action that still updates the status
- [ ] Filter chips and the date picker are restyled and functional
- [ ] Empty state is restyled
- [ ] `dotnet build` and `dotnet test` pass