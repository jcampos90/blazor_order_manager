# 03: "Hoy" dashboard: urgency grouping

**What to build:** The baker's daily screen groups production into urgency sections — Atrasados · Ahora · Hoy · Mañana · Próximos — so prioritization needs no mental math. Stat cards are restyled, skeleton placeholders show while data loads, and overdue production stays clearly flagged. Includes a pure urgency-bucket function (`Services` layer, beside `PrepSchedule`) that is unit-tested against boundary cases.

**Blocked by:** 01, 02.

**Status:** ready-for-agent

- [ ] Pure urgency-bucket function classifies a start-by time: Atrasado (overdue), Ahora (≤ now + 60 min), Hoy (later today), Mañana (tomorrow), Próximos (after tomorrow)
- [ ] xUnit boundary tests cover overdue / now / +60 min / today / tomorrow / beyond
- [ ] Dashboard renders production grouped under the five section headers, ordered by start-by within each section
- [ ] Overdue items are flagged (section + badge); stat cards restyled
- [ ] Skeleton placeholders show while the dashboard loads; the empty state still works
- [ ] `dotnet build` and `dotnet test` pass