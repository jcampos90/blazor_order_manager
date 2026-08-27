# 01: Design tokens & app shell

**What to build:** The app's global design foundation. A neutral token system (white/stone surfaces, a single emerald accent, airy spacing/radius/shadow scale) with Space Grotesk + Inter replacing the serif display font, plus a manual light/dark theme toggle that persists the baker's choice and defaults to light. The shell is restyled: sidebar with a redrawn, toned-down brand ("Panadería · Gestión de encargos") and nav, and a topbar that keeps the date, identity and sign-out and gains the theme toggle. Every page inherits typography, colors and theme switching from here.

**Blocked by:** None (can start immediately).

**Status:** ready-for-agent

- [ ] CSS design tokens define the neutral palette, emerald accent, spacing/radius/shadow scale, and the dark-theme variable set
- [ ] Space Grotesk + Inter replace Fraunces/Inter; Fraunces is no longer loaded
- [ ] Theme toggle in the topbar switches light/dark; the choice persists across reloads (`localStorage`) and defaults to light
- [ ] All colors flow from CSS variables — no hardcoded color leaks across themes
- [ ] Sidebar shows the redrawn brand and restyled nav; topbar keeps date, identity and sign-out and adds the theme toggle
- [ ] `dotnet build` and `dotnet test` pass