# UI Redesign — modern, clean, beautiful

**Status:** ready-for-agent

## Problem Statement

The app works, but its warm "artisan bakery" styling (cream/terracotta/serif), inline page forms, text-only loading states, and flat production table make it feel dated. The baker wants the whole app to look **modern, clean and professional** while keeping its bakery identity — and to prioritize production at a glance without doing mental math.

## Solution

A neutral, modern SaaS redesign built on the existing hand-rolled CSS system: white/stone surfaces, a single emerald accent, Inter + Space Grotesk typography, airy spacing and subtle motion. A manual light/dark toggle, skeleton loading, modal-based quick CRUD, an urgency-grouped "Hoy" dashboard, and a live "Cuándo iniciar" summary in the order editor.

## User Stories

1. As a baker, I want the whole app to share one clean, modern visual style, so that it feels professional and pleasant to use all day.
2. As a baker, I want a light and a dark theme with a manual toggle in the topbar, so that I can read the app comfortably in any lighting.
3. As a baker, I want the chosen theme remembered between sessions, so that I don't have to reset it every day.
4. As a baker, I want the "Hoy" dashboard to group production by urgency (Atrasados · Ahora · Hoy · Mañana), so that I can prioritize at a glance without mental math.
5. As a baker, I want the dashboard to show skeleton placeholders while loading, so that the screen doesn't flash raw text.
6. As a baker, I want overdue production clearly flagged, so that I know what is slipping immediately.
7. As a baker, I want to immediately see which preparations must start now, so that nothing is late.
8. As a baker, I want the encargos list to use clean rows with quick actions, so that I can scan and act efficiently.
9. As a baker, I want to mark an encargo as delivered with one clear button, so that the flow stays fast.
10. As a baker, I want the order editor to show the "Cuándo iniciar" time for each product as I set the delivery time, so that I can confirm the schedule while creating the encargo.
11. As a baker, I want the editor to keep the delivery date/time, status and total visible while I edit lines, so that I keep context.
12. As a baker, I want to create and edit products and customers in modal dialogs, so that the list pages stay uncluttered.
13. As a baker, I want clear, inviting empty states, so that I know what to do when there is no data.
14. As a baker, I want visible focus states and subtle hover feedback on all controls, so that the app feels responsive and accessible.
15. As a baker, I want consistent spacing, radius and shadows across every page, so that the app looks cohesive.
16. As a baker, I want the sidebar and topbar to feel refreshed but recognizable, so that the app keeps its identity.

## Implementation Decisions

- Keep the hand-rolled CSS system; extend the design tokens to a neutral palette with an emerald accent, Space Grotesk + Inter, an airy spacing/radius/shadow scale, and dark-theme variables.
- Dark mode: `data-theme` on the root element, manual toggle in the topbar, default light, persisted in `localStorage`; all colors flow from variables so no component duplicates the palette.
- Replace the Fraunces/Inter font load with Space Grotesk (headings) + Inter (body).
- Hand-rolled accessible modal pattern for Products & Customers create/edit; the order editor remains a full page.
- Dashboard: a pure urgency-bucket function — Atrasado (overdue), Ahora (startBy ≤ now + 60 min), Hoy (later today), Mañana (tomorrow), Próximos (after tomorrow) — groups production rows under section headers; existing query and stat cards are retained.
- Order editor: two-column layout with a sticky sidebar showing delivery date/time, status, total, and a live "Cuándo iniciar" summary computing `PrepSchedule.StartBy(DeliveryAt, product.PrepHours)` per line.
- Skeleton loading (pure CSS shimmer) for dashboard and list loads; empty states restyled with icon tiles and clear CTA hierarchy.
- Row actions become icon-only ghosts except "Marcar entregado" (labeled success button).
- Topbar keeps date, identity and sign-out and gains the theme toggle.
- UI copy uses existing domain vocabulary (encargo, cliente, producto, renglón, entrega, atrasado); the editor summary is labeled **"Cuándo iniciar"**, never "production plan" (per `CONTEXT.md`).

## Testing Decisions

- Only external behavior is tested; CSS and markup are verified by `dotnet build` + `dotnet test` + a manual run (no bUnit in the repo).
- Seam 1: the new pure urgency-bucket function is tested with xUnit against boundary cases (overdue / now / +60 min / today / tomorrow / beyond).
- Seam 2: existing `PrepSchedule.StartBy` tests already cover the editor's start-by computation; no new tests required there.
- Prior art: `PrepScheduleTests`, `DashboardServiceTests` (xUnit, InMemory EF via `TestFactory`).

## Out of Scope

- Auto dark mode (no system-preference following; manual toggle only).
- New dependencies (no component library, no bUnit, no Tailwind).
- Backend/domain changes beyond the new bucket classification — no schema changes, no new endpoints.
- Heavy animation, illustrations, or copy overhaul.
- Auth UI changes beyond restyling existing controls (ADR-0001 untouched).
- i18n — Spanish (AR) only.

## Further Notes

- The app requires Clerk user-secrets to run; manual verification needs them configured.
- All labels align with the `CONTEXT.md` glossary; `PrepSchedule` avoids "production plan".