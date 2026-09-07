# 07: Topbar `Mi cuenta` link + sidebar `Usuarios` link

**What to build:** Topbar gains a `Mi cuenta` button next to the identity label (links to `/cuenta`). Sidebar gains a `Usuarios` link inside an `<AuthorizeView Roles="Owner">` block.

**Blocked by:** 03, 05, 06 (link is wired up after pages exist)

**Status:** pending

- [ ] `Components/Layout/MainLayout.razor` topbar: a `Mi cuenta` button / link before `Cerrar sesión`, pointing to `/cuenta`. Reuse the existing `btn btn-ghost btn-sm` style.
- [ ] `Components/Layout/NavMenu.razor` `.nav` section: append an `<AuthorizeView Roles="Owner">` block containing a `NavLink` to `usuarios` with the same iconography as the other nav items. The icon can be a generic users SVG (two stacked silhouettes).
- [ ] `dotnet build` passes; smoke test: signed in as Owner, both `Mi cuenta` and `Usuarios` are reachable; signed in as Staff, only `Mi cuenta` is reachable and `Usuarios` is not in the DOM.
