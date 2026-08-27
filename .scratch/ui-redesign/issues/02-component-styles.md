# 02: Component styles

**What to build:** Every shared UI component is restyled onto the new design system so all pages inherit a modern, clean, airy look at once: buttons (primary/outline/ghost/success, sm and icon variants), cards, stats, tables (headers, rows, footer, muted/overdue rows), badges, chips, form controls with focus rings, alerts, empty states, and skeleton loading placeholders. Subtle hover/press motion and `:focus-visible` rings throughout.

**Blocked by:** 01.

**Status:** ready-for-agent

- [ ] Buttons, cards, stats, tables, badges, chips, form controls + focus rings, alerts and empty states all render in the new system
- [ ] Skeleton shimmer styles exist and can be dropped into any loading state
- [ ] Subtle hover/press transitions and visible focus rings on every interactive component
- [ ] Responsive behavior is preserved (mobile nav/sidebar, stacked forms)
- [ ] `dotnet build` and `dotnet test` pass